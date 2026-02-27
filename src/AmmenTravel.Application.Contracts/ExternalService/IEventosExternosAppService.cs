using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.ExternalService
{
    public interface IEventosExternosAppService : IApplicationService
    {
        Task<List<EventoTicketmasterDto>> ObtenerEventosPorUbicacionAsync(string latitud, string longitud);
    }
}