using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using AmmenTravel.Opiniones.OpinionesDTO;

namespace AmmenTravel.Opiniones
{
    public interface IOpinionAppService : IApplicationService
    {
        Task <OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input);
    }
}