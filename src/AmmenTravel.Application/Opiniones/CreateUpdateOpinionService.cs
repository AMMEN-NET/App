using AmmenTravel.Opiniones.OpinionesDTO;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;


namespace AmmenTravel.Opiniones
{
    public class CrearOpinionService : ICreateUpdateOpinion
    {
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly ICurrentUser _currentUser;

        public CrearOpinionService(IRepository<Opinion, Guid> opinionRepository, ICurrentUser currentUser)
        {
            _opinionRepository = opinionRepository;
            _currentUser = currentUser;
        }

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new AbpAuthorizationException("Debes iniciar sesión para crear una opinión.");

            var userId = _currentUser.Id ?? throw new AbpAuthorizationException("No se pudo obtener el usuario.");


            // Esto es para evitar que un usuario califique el mismo destino más de una vez

            var opinionExistente = await _opinionRepository.FirstOrDefaultAsync(
            o => o.DestinoTuristicoId == input.DestinoTuristicoId && o.UserId == userId );

            if (opinionExistente != null)
                throw new UserFriendlyException("Ya has calificado este destino.");

            var opinion = new Opinion(input.DestinoTuristicoId, userId, input.Puntuacion, input.Comentario);
            await _opinionRepository.InsertAsync(opinion, autoSave: true);

            return new OpinionDto
            {
                Id = opinion.Id,
                DestinoTuristicoId = opinion.DestinoTuristicoId,
                UserId = opinion.UserId,
                Puntuacion = opinion.Puntuacion,
                Comentario = opinion.Comentario
            };
        }
    }
}
