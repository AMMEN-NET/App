using AmmenTravel.Opiniones.OpinionesDTO;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using AmmenTravel.Destinos;


namespace AmmenTravel.Opiniones
{
    public class CrearOpinionService : ICreateUpdateOpinion
    {
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly ICurrentUser _currentUser;

        public CrearOpinionService(
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            ICurrentUser currentUser)
        {
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _currentUser = currentUser;
        }

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new AbpAuthorizationException("Debes iniciar sesión para crear una opinión.");

            var userId = _currentUser.Id ?? throw new AbpAuthorizationException("No se pudo obtener el usuario.");

            // Validar que el destino existe
            var destino = await _destinoRepository.GetAsync(input.DestinoTuristicoId);

            // Verificar si el usuario ya opinó sobre este destino
            var opinionExistente = await _opinionRepository.FirstOrDefaultAsync(
                o => o.DestinoTuristicoId == input.DestinoTuristicoId && o.UserId == userId);

            if (opinionExistente != null)
                throw new UserFriendlyException($"Ya has calificado {destino.Nombre}. Puedes actualizar tu opinión si lo deseas.");

            var opinion = new Opinion(input.DestinoTuristicoId, userId, input.Puntuacion, input.Comentario);
            await _opinionRepository.InsertAsync(opinion, autoSave: true);

            return new OpinionDto
            {
                Id = opinion.Id,
                DestinoTuristicoId = opinion.DestinoTuristicoId,
                NombreDestino = destino.Nombre,
                UserId = opinion.UserId,
                Puntuacion = opinion.Puntuacion,
                Comentario = opinion.Comentario,
                CreationTime = opinion.CreationTime
            };
        }

        public async Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new AbpAuthorizationException("Debes iniciar sesión para actualizar una opinión.");

            var userId = _currentUser.Id ?? throw new AbpAuthorizationException("No se pudo obtener el usuario.");

            // Obtenemos la opinión existente
            var opinion = await _opinionRepository.GetAsync(opinionId);

            // Validamos que el usuario sea el dueño de la opinión
            if (opinion.UserId != userId)
                throw new AbpAuthorizationException("No tienes permiso para actualizar esta opinión.");

            // Obtenemos el destino para obtener su nombre
            var destino = await _destinoRepository.GetAsync(opinion.DestinoTuristicoId);

            // Actualizamos los campos
            opinion.Puntuacion = input.Puntuacion;
            opinion.Comentario = input.Comentario;

            // Guardamos los cambios
            await _opinionRepository.UpdateAsync(opinion, autoSave: true);

            return new OpinionDto
            {
                Id = opinion.Id,
                DestinoTuristicoId = opinion.DestinoTuristicoId,
                NombreDestino = destino.Nombre,
                UserId = opinion.UserId,
                Puntuacion = opinion.Puntuacion,
                Comentario = opinion.Comentario,
                CreationTime = opinion.CreationTime
            };
        }

        public async Task EliminarOpinionAsync(Guid opinionId)
        {
            if (!_currentUser.IsAuthenticated)
                throw new AbpAuthorizationException("Debes iniciar sesión para eliminar una opinión.");

            var userId = _currentUser.Id ?? throw new AbpAuthorizationException("No se pudo obtener el usuario.");

            // Obtenemos la opinión existente
            var opinion = await _opinionRepository.GetAsync(opinionId);

            // Validamos que el usuario sea el dueño de la opinión
            if (opinion.UserId != userId)
                throw new AbpAuthorizationException("No tienes permiso para eliminar esta opinión.");

            // Eliminamos la opinión
            await _opinionRepository.DeleteAsync(opinion, autoSave: true);
        }
    }
}