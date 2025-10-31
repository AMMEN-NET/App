using AmmenTravel.Opiniones.OpinionesDTO;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Users;
using Volo.Abp.Domain.Repositories;

namespace AmmenTravel.Opiniones
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class OpinionAppService : ApplicationService, IOpinionAppService
    {
        private readonly ICreateUpdateOpinion _crearOpinionService;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly ICurrentUser _currentUser;

        public OpinionAppService(
            ICreateUpdateOpinion crearOpinionService,
            IRepository<Opinion, Guid> opinionRepository,
            ICurrentUser currentUser)
        {
            _crearOpinionService = crearOpinionService;
            _opinionRepository = opinionRepository;
            _currentUser = currentUser;
        }

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            return await _crearOpinionService.CrearOpinionAsync(input);
        }

        public async Task<List<OpinionDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            // 1️⃣ Verificar autenticación
            if (!_currentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("Debe estar autenticado para ver sus opiniones.");
            }

            // 2️⃣ Validar que solo consulte su propia información
            if (_currentUser.Id != usuarioId)
            {
                throw new AbpAuthorizationException("No tiene permiso para ver las opiniones de otro usuario.");
            }

            // 3️⃣ Obtener opiniones filtradas por usuario
            var opiniones = await _opinionRepository.GetListAsync(o => o.UserId == usuarioId);

            // 4️⃣ Mapearlas al DTO
            return opiniones.Select(o => new OpinionDto
            {
                Id = o.Id,
                UserId = o.UserId,
                DestinoTuristicoId = o.DestinoTuristicoId,
                Comentario = o.Comentario,
                Puntuacion = o.Puntuacion
            }).ToList();
        }
    }
}
