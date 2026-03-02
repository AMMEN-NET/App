using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Guids;
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
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<NotificadorEventosService> _logger;

        public NotificadorEventosService(
            IRepository<LineaListaFavorito, Guid> lineaFavoritosRepo,
            IRepository<ListaFavorito, Guid> listaFavoritosRepo,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IEventosExternosAppService eventosExternosService,
            IRepository<Notificacion, Guid> notificacionRepository,
            IGuidGenerator guidGenerator,
            ILogger<NotificadorEventosService> logger)
        {
            _lineaFavoritosRepo = lineaFavoritosRepo;
            _listaFavoritosRepo = listaFavoritosRepo;
            _destinoRepository = destinoRepository;
            _eventosExternosService = eventosExternosService;
            _notificacionRepository = notificacionRepository;
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

                    var eventos = await _eventosExternosService.ObtenerEventosPorUbicacionAsync(lat, lon);

                    if (eventos != null && eventos.Any())
                    {
                        var lineasDeEsteDestino = await _lineaFavoritosRepo.GetListAsync(l => l.DestinoTuristicoId == destinoId);

                        foreach (var linea in lineasDeEsteDestino)
                        {
                            var listaFavorito = await _listaFavoritosRepo.GetAsync(linea.ListaFavoritoId);
                            var userId = listaFavorito.UserId;

                            int cantidadEventos = eventos.Count;
                            string mensaje = cantidadEventos == 1
                                ? $"¡Actualización! Hay un evento próximo en {destino.Nombre}. ¡Revisalo!"
                                : $"¡Actualización! Hay {cantidadEventos} eventos próximos en {destino.Nombre}.";

                            var notificacion = new Notificacion(
                                _guidGenerator.Create(),
                                userId,
                                $"Novedades en {destino.Nombre} 🎟️",
                                mensaje,
                                TipoNotificacion.Social,
                                "/favoritos",
                                "fa-ticket-alt"
                            );

                            await _notificacionRepository.InsertAsync(notificacion);
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