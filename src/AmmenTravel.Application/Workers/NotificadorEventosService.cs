using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Guids;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using AmmenTravel.ExternalService;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Notificaciones;
using AmmenTravel.Destinos;

namespace AmmenTravel.BackgroundWorkers
{
    // ITransientDependency hace que ABP lo registre automáticamente
    public class NotificadorEventosService : ITransientDependency
    {
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritosRepo;
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritosRepo;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IEventosExternosAppService _eventosExternosService;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IRepository<HistorialNotificacionEvento, Guid> _historialRepo;
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepo;
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<NotificadorEventosService> _logger;

        public NotificadorEventosService(
            IRepository<LineaListaFavorito, Guid> lineaFavoritosRepo,
            IRepository<ListaFavorito, Guid> listaFavoritosRepo,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IEventosExternosAppService eventosExternosService,
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<HistorialNotificacionEvento, Guid> historialRepo,
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepo,
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IdentityUserManager userManager,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<NotificadorEventosService> logger)
        {
            _lineaFavoritosRepo = lineaFavoritosRepo;
            _listaFavoritosRepo = listaFavoritosRepo;
            _destinoRepository = destinoRepository;
            _eventosExternosService = eventosExternosService;
            _notificacionRepository = notificacionRepository;
            _historialRepo = historialRepo;
            _preferenciasRepo = preferenciasRepo;
            _colaEmailRepo = colaEmailRepo;
            _userManager = userManager;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        // Es VITAL que tenga [UnitOfWork] y sea virtual
        [UnitOfWork]
        public virtual async Task ProcesarEventosAsync()
        {
            try
            {
                var queryLineas = await _lineaFavoritosRepo.GetQueryableAsync();

                var destinosEnFavoritos = await queryLineas
                    .Select(l => l.DestinoTuristicoId)
                    .Distinct()
                    .ToListAsync();

                foreach (var destinoId in destinosEnFavoritos)
                {
                    var destino = await _destinoRepository.GetAsync(destinoId);
                    string lat = destino.Latitud.ToString(CultureInfo.InvariantCulture);
                    string lon = destino.Longitud.ToString(CultureInfo.InvariantCulture);

                    // 1. Buscamos TODOS los eventos de Ticketmaster
                    var eventos = await _eventosExternosService.ObtenerEventosPorUbicacionAsync(lat, lon);

                    if (eventos != null && eventos.Any())
                    {
                        var lineasDeEsteDestino = await _lineaFavoritosRepo.GetListAsync(l => l.DestinoTuristicoId == destinoId);

                        foreach (var linea in lineasDeEsteDestino)
                        {
                            // Obtenemos el IQueryable para poder usar funciones nativas de Entity Framework
                            var listasQuery = await _listaFavoritosRepo.GetQueryableAsync();

                            // Usamos IgnoreQueryFilters() para saltarnos la restricción de IUserOwned momentáneamente
                            var listaFavorito = await listasQuery
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(l => l.Id == linea.ListaFavoritoId);

                            if (listaFavorito == null)
                            {
                                continue; // Si por algún motivo la BD está inconsistente, saltamos a la siguiente
                            }

                            var userId = listaFavorito.UserId;

                            // 2. Buscamos en nuestra memoria los eventos de ESTE destino que ya le avisamos a ESTE usuario
                            var historialAnterior = await _historialRepo.GetListAsync(h =>
                                h.UserId == userId && h.DestinoTuristicoId == destinoId);

                            var idsYaNotificados = historialAnterior.Select(h => h.EventoTicketmasterId).ToList();

                            // 3. LA MAGIA: Filtramos solo los eventos de Ticketmaster cuyo ID no esté en nuestra memoria
                            var eventosNuevos = eventos.Where(e => !idsYaNotificados.Contains(e.Id)).ToList();

                            // 4. Solo enviamos notificación si realmente hay NUEVOS
                            if (eventosNuevos.Any())
                            {
                                int cantidadNuevos = eventosNuevos.Count;
                                string titulo = $"Novedades en {destino.Nombre} 🎟️";
                                string mensaje = cantidadNuevos == 1
                                    ? $"¡Actualización! Hay 1 evento NUEVO en {destino.Nombre}. ¡Revisalo!"
                                    : $"¡Actualización! Se agregaron {cantidadNuevos} eventos nuevos en {destino.Nombre}.";

                                // Obtenemos las preferencias del usuario (o valores por defecto)
                                var preferencias = await _preferenciasRepo.FirstOrDefaultAsync(p => p.UserId == userId);
                                bool enPantalla = preferencias?.EnPantalla ?? true;
                                bool porEmail = preferencias?.PorEmail ?? true;
                                FrecuenciaNotificacion frecuencia = preferencias?.Frecuencia ?? FrecuenciaNotificacion.Inmediata;

                                if (frecuencia == FrecuenciaNotificacion.Inmediata)
                                {
                                    // === MODO INMEDIATO: todo al instante ===

                                    // Campanita
                                    if (enPantalla)
                                    {
                                        var notificacion = new Notificacion(
                                            _guidGenerator.Create(),
                                            userId,
                                            titulo,
                                            mensaje,
                                            TipoNotificacion.Social,
                                            "/favoritos",
                                            "fa-ticket-alt"
                                        );
                                        await _notificacionRepository.InsertAsync(notificacion);
                                    }

                                    // Email
                                    if (porEmail)
                                    {
                                        var user = await _userManager.FindByIdAsync(userId.ToString());
                                        var email = user?.Email;
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            await _emailSender.SendAsync(email, titulo, mensaje);
                                        }
                                    }
                                }
                                else
                                {
                                    // === MODO SEMANAL: todo se encola para el domingo ===
                                    var user = await _userManager.FindByIdAsync(userId.ToString());
                                    var email = user?.Email ?? "";

                                    var colaEmail = new ColaResumenSemanalEmail(
                                        _guidGenerator.Create(),
                                        userId,
                                        email,
                                        titulo,
                                        mensaje
                                    );
                                    await _colaEmailRepo.InsertAsync(colaEmail);
                                }

                                // 5. Guardamos en la memoria los IDs nuevos para no volver a avisar mañana
                                var nuevosRegistrosHistorial = eventosNuevos.Select(e => new HistorialNotificacionEvento(
                                    _guidGenerator.Create(),
                                    userId,
                                    destinoId,
                                    e.Id
                                )).ToList();

                                await _historialRepo.InsertManyAsync(nuevosRegistrosHistorial);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar actualizaciones de Ticketmaster");
            }
        }
    }
}