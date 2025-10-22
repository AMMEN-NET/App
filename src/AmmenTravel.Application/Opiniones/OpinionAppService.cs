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

        [AllowAnonymous]
        public async Task CrearOpinionAsync(Guid destinoId, ValorPuntuacion puntuacion, string comentario)
        {
            // Si permites anónimos, hay que evitar usar CurrentUser.GetId() sin comprobar.
            Guid userId;
            if (CurrentUser.IsAuthenticated)
            {
                userId = CurrentUser.GetId();
            }
            else
            {
                // Asignar un valor por defecto o lanzar si requieres userId.
                userId = Guid.Empty; // <-- ajustar según lógica de negocio
            }

            var opinion = new Opinion(destinoId, userId, puntuacion, comentario);
            await _opinionRepository.InsertAsync(opinion);
        }
    }
}