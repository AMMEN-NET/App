using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Viajeros
{
    public interface IViajerosAppService : IApplicationService
    {
        Task<List<ViajeroDto>> GetListAsync(string filtro = null);
    }
}