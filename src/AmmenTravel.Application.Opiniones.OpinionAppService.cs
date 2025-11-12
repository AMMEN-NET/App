using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmmenTravel.InterfaceOpinionesAppService;
using AmmenTravel.OpinionesDTO;
using AmmenTravel.Opiniones;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;

namespace AmmenTravel.Opiniones
{
    [Authorize]
    public class OpinionAppService : AmmenTravelAppService, IOpinionAppService
    {
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        public OpinionAppService(IRepository<Opinion, Guid> opinionRepository)
        {
            _opinionRepository = opinionRepository;
        }

        public async Task CrearOpinionAsync(Guid destinoId, ValorPuntuacion puntuacion, string comentario)
        {
            // Requerir autenticación explícita: si no está autenticado lanza excepción
            if (!CurrentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("Autenticación requerida para crear opiniones.");
            }

            var userId = CurrentUser.GetId(); // ahora siempre existe porque el usuario está autenticado

            var opinion = new Opinion(destinoId, userId, puntuacion, comentario);

            await _opinionRepository.InsertAsync(opinion);
            // El UoW/ABP guardará al final de la llamada automática
        }

        [AllowAnonymous]
        public async Task<List<OpinionDto>> GetOpinionesPorDestinoAsync(Guid destinoId)
        {
            var list = await _opinionRepository.GetListAsync(op => op.DestinoTuristicoId == destinoId);
            return ObjectMapper.Map<List<Opinion>, List<OpinionDto>>(list);
        }
    }
}