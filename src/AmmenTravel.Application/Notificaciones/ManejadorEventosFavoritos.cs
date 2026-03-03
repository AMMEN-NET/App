using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AmmenTravel.Destinos;
using AmmenTravel.ExternalService;
using AmmenTravel.ListaDeFavoritos;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace AmmenTravel.Notificaciones
{
    public class ManejadorEventosFavoritos : ILocalEventHandler<DestinoAgregadoAFavoritosEto>, ITransientDependency
    {
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IEventosExternosAppService _eventosExternosService;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IRepository<PreferenciasNotificacion, Guid> _preferenciasRepo;
        private readonly IRepository<ColaResumenSemanalEmail, Guid> _colaEmailRepo;
        private readonly IdentityUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ManejadorEventosFavoritos> _logger;

        public ManejadorEventosFavoritos(
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IEventosExternosAppService eventosExternosService,
            IRepository<Notificacion, Guid> notificacionRepository,
            IRepository<PreferenciasNotificacion, Guid> preferenciasRepo,
            IRepository<ColaResumenSemanalEmail, Guid> colaEmailRepo,
            IdentityUserManager userManager,
            IEmailSender emailSender,
            IGuidGenerator guidGenerator,
            ILogger<ManejadorEventosFavoritos> logger)
        {
            _destinoRepository = destinoRepository;
            _eventosExternosService = eventosExternosService;
            _notificacionRepository = notificacionRepository;
            _preferenciasRepo = preferenciasRepo;
            _colaEmailRepo = colaEmailRepo;
            _userManager = userManager;
            _emailSender = emailSender;
            _guidGenerator = guidGenerator;
            _logger = logger;
        }

        public async Task HandleEventAsync(DestinoAgregadoAFavoritosEto eventData)
        {
            try
            {
                var destino = await _destinoRepository.GetAsync(eventData.DestinoId);

                string lat = destino.Latitud.ToString(CultureInfo.InvariantCulture);
                string lon = destino.Longitud.ToString(CultureInfo.InvariantCulture);

                var eventos = await _eventosExternosService.ObtenerEventosPorUbicacionAsync(lat, lon);

                if (eventos != null && eventos.Any())
                {
                    int cantidadEventos = eventos.Count;
                    string titulo = $"¡Planazo en {destino.Nombre}! 🎟️";
                    string mensaje = cantidadEventos == 1
                        ? $"Acabas de guardar {destino.Nombre} y hay un evento próximo esperandote. ¡Revisalo antes de que se agote!"
                        : $"Acabas de guardar {destino.Nombre} y hay {cantidadEventos} eventos próximos esperando. ¡Sacá tus tickets!";

                    // Obtenemos las preferencias del usuario (o valores por defecto)
                    var preferencias = await _preferenciasRepo.FirstOrDefaultAsync(p => p.UserId == eventData.UserId);
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
                                eventData.UserId,
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
                            var user = await _userManager.FindByIdAsync(eventData.UserId.ToString());
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
                        var user = await _userManager.FindByIdAsync(eventData.UserId.ToString());
                        var email = user?.Email ?? "";

                        var colaEmail = new ColaResumenSemanalEmail(
                            _guidGenerator.Create(),
                            eventData.UserId,
                            email,
                            titulo,
                            mensaje
                        );
                        await _colaEmailRepo.InsertAsync(colaEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en Background al procesar eventos de Ticketmaster para el destino {eventData.DestinoId}");
            }
        }
    }
}