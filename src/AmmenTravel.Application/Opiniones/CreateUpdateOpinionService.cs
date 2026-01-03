using AmmenTravel.Opiniones.OpinionesDTO;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Data; // <--- AGREGAR ESTO
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
        private readonly IDataFilter _dataFilter; // <--- AGREGAR ESTO

        public CrearOpinionService(
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            ICurrentUser currentUser,
            IDataFilter dataFilter) // <--- INYECTARLO AQUÍ
        {
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _currentUser = currentUser;
            _dataFilter = dataFilter;
        }

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            if (!_currentUser.IsAuthenticated)
                throw new AbpAuthorizationException("Debes iniciar sesión para crear una opinión.");

            var userId = _currentUser.Id ?? throw new AbpAuthorizationException("No se pudo obtener el usuario.");
            var destino = await _destinoRepository.GetAsync(input.DestinoTuristicoId);

            // --- LÓGICA MODIFICADA ---

            // Usamos el DataFilter para buscar incluso opiniones borradas (Soft Delete)
            Opinion opinionExistente;
            using (_dataFilter.Disable<ISoftDelete>())
            {
                opinionExistente = await _opinionRepository.FirstOrDefaultAsync(
                    o => o.DestinoTuristicoId == input.DestinoTuristicoId && o.UserId == userId);
            }

            if (opinionExistente != null)
            {
                // CASO 1: La opinión existe y está activa (No borrada)
                if (!opinionExistente.IsDeleted)
                {
                    throw new UserFriendlyException($"Ya calificaste {destino.Nombre}. Podes actualizar tu opinión en la sección 'Mis calificaciones' si lo deseas.");
                }

                // CASO 2: La opinión existía pero estaba BORRADA -> LA RESTAURAMOS
                // "Damos de alta" nuevamente la opinión antigua con los nuevos valores
                opinionExistente.IsDeleted = false; // Restaurar
                opinionExistente.Puntuacion = input.Puntuacion;
                opinionExistente.Comentario = input.Comentario;

                await _opinionRepository.UpdateAsync(opinionExistente, autoSave: true);

                return new OpinionDto
                {
                    Id = opinionExistente.Id,
                    DestinoTuristicoId = opinionExistente.DestinoTuristicoId,
                    NombreDestino = destino.Nombre,
                    UserId = opinionExistente.UserId,
                    Puntuacion = opinionExistente.Puntuacion,
                    Comentario = opinionExistente.Comentario,
                    CreationTime = opinionExistente.CreationTime // Conserva la fecha original
                };
            }

            // CASO 3: No existe ninguna (ni activa ni borrada) -> CREAR NUEVA
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