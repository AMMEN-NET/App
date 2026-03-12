using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Notificaciones
{
    public interface IPreferenciasNotificacionAppService : IApplicationService
    {
        Task<PreferenciasNotificacionDto> GetMiPreferenciaAsync();
        Task<PreferenciasNotificacionDto> UpdateMiPreferenciaAsync(UpdatePreferenciasDto input);
    }
}
