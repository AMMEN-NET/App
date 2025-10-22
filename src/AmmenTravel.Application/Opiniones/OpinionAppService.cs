using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;

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
            var userId = CurrentUser.GetId(); // ✅ obtiene el ID del usuario autenticado

            var opinion = new Opinion(destinoId, userId, puntuacion, comentario);
            await _opinionRepository.InsertAsync(opinion);
        }
    }
}