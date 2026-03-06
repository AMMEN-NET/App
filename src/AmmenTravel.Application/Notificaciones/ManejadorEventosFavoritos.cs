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
using Volo.Abp.EventBus;
using Volo.Abp.Guids;

namespace AmmenTravel.Notificaciones
{
    public class ManejadorEventosFavoritos : ILocalEventHandler<DestinoAgregadoAFavoritosEto>, ITransientDependency
    {
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IEventosExternosAppService _eventosExternosService;
        private readonly IRepository<Notificacion, Guid> _notificacionRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ManejadorEventosFavoritos> _logger;

        public ManejadorEventosFavoritos(
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IEventosExternosAppService eventosExternosService,
            IRepository<Notificacion, Guid> notificacionRepository,
            IGuidGenerator guidGenerator,
            ILogger<ManejadorEventosFavoritos> logger)
        {
            _destinoRepository = destinoRepository;
            _eventosExternosService = eventosExternosService;
            _notificacionRepository = notificacionRepository;
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
                    string mensaje;

                    if (cantidadEventos == 1)
                    {
                        mensaje = $"Acabas de guardar {destino.Nombre} y hay un evento próximo espereandote. ¡Revisalo antes de que se agote!";
                    }
                    else
                    {
                        mensaje = $"Acabas de guardar {destino.Nombre} y hay {cantidadEventos} eventos próximos esperando. ¡Sacá tus tickets!";
                    }

                    var notificacion = new Notificacion(
                        _guidGenerator.Create(),
                        eventData.UserId,
                        $"¡Planazo en {destino.Nombre}! 🎟️",
                        mensaje,
                        TipoNotificacion.Social,
                        "/favoritos", 
                        "fa-ticket-alt"
                    );

                    await _notificacionRepository.InsertAsync(notificacion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en Background al procesar eventos de Ticketmaster para el destino {eventData.DestinoId}");
            }
        }
    }
}