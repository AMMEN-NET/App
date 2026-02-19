using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Estadisticas
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<DashboardResumenDto> GetResumenAsync();
    }
}