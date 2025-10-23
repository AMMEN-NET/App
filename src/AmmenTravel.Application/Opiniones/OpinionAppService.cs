using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;

namespace AmmenTravel.Opiniones
{
    [Authorize]
    public class OpinionAppService : ApplicationService
    {
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        public OpinionAppService(IRepository<Opinion, Guid> opinionRepository)
        {
            _opinionRepository = opinionRepository;
        }

        public async Task CrearOpinionAsync(Guid destinoId, ValorPuntuacion puntuacion, string comentario)
        {
            // No se permiten anónimos: asegurar que el usuario esté autenticado
            if (!CurrentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("Se requiere autenticación para crear una opinión.");
            }

            var userId = CurrentUser.GetId();

            var opinion = new Opinion(destinoId, userId, puntuacion, comentario);
            await _opinionRepository.InsertAsync(opinion);
        }
    }
}