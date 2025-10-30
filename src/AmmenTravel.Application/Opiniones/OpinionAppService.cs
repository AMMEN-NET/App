using AmmenTravel.Opiniones.OpinionesDTO;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;


namespace AmmenTravel.Opiniones
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class OpinionAppService : ApplicationService, IOpinionAppService
    {
        private readonly ICreateUpdateOpinion _crearOpinionService;

        public OpinionAppService(ICreateUpdateOpinion crearOpinionService)
        {
            _crearOpinionService = crearOpinionService;
        }

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            return await _crearOpinionService.CrearOpinionAsync(input);
        }
    }
}
