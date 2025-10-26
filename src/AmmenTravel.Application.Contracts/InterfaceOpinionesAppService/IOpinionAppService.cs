using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using AmmenTravel.OpinionesDTO;

namespace AmmenTravel.InterfaceOpinionesAppService
{
    public interface IOpinionAppService : IApplicationService
    {
        Task CrearOpinionAsync(createUpdateOpinionDto input);
        Task<List<OpinionDto>> GetOpinionesPorDestinoAsync(Guid destinoId);
    }
}