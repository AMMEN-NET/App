using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AmmenTravel.Cuentas
{
    public interface ICuentaAppService : IApplicationService
    {
        Task EliminarMiCuentaAsync();
    }
}