using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace AmmenTravel.Opiniones
{
    // Forzamos explícitamente el esquema de autenticación de OpenIddict (Bearer)
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class OpinionAppService : ApplicationService
    {
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        public OpinionAppService(IRepository<Opinion, Guid> opinionRepository)
        {
            _opinionRepository = opinionRepository;
        }

        // Mantengo la misma firma para que Swagger siga mostrando:
        // POST /api/app/opinion/crear-opinion/{destinoId}
        public async Task<Guid> CrearOpinionAsync(Guid destinoId, ValorPuntuacion puntuacion, string comentario)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("Se requiere autenticación para crear una opinión.");
            }

            var userId = CurrentUser.Id;
            if (userId == null || userId == Guid.Empty)
            {
                throw new AbpAuthorizationException("No se ha podido obtener el identificador del usuario desde el token.");
            }

            var opinion = new Opinion(destinoId, userId.Value, puntuacion, comentario);

            await _opinionRepository.InsertAsync(opinion, autoSave: true);

            return opinion.Id;
        }
    }
}