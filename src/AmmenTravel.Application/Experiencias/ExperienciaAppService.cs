using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp;

namespace AmmenTravel.Experiencias
{
    [Authorize] // Requiere login para todo
    public class ExperienciaAppService : AmmenTravelAppService, IExperienciaAppService
    {
        private readonly IRepository<Experiencia, Guid> _experienciaRepository;
        private readonly IIdentityUserRepository _userRepository;

        public ExperienciaAppService(
            IRepository<Experiencia, Guid> experienciaRepository,
            IIdentityUserRepository userRepository)
        {
            _experienciaRepository = experienciaRepository;
            _userRepository = userRepository;
        }

        public async Task<ExperienciaDto> CreateAsync(CreateUpdateExperienciaDto input)
        {
            // Creamos la entidad
            var experiencia = new Experiencia(
                GuidGenerator.Create(),
                input.DestinoId,
                input.Valoracion,
                input.Comentario
            );

            await _experienciaRepository.InsertAsync(experiencia);

            // Mapeamos manualmente para devolver rápido (o usa ObjectMapper si lo configuraste)
            var usuarioActual = await _userRepository.GetAsync(CurrentUser.Id.Value);

            return new ExperienciaDto
            {
                Id = experiencia.Id,
                DestinoId = experiencia.DestinoId,
                Valoracion = experiencia.Valoracion,
                Comentario = experiencia.Comentario,
                CreationTime = DateTime.Now,
                CreatorId = usuarioActual.Id,
                UserName = usuarioActual.UserName
            };
        }

        public async Task<ExperienciaDto> UpdateAsync(Guid id, CreateUpdateExperienciaDto input)
        {
            var experiencia = await _experienciaRepository.GetAsync(id);

            // Validación: Solo el dueño puede editar
            if (experiencia.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para editar esta experiencia.");
            }

            experiencia.Valoracion = input.Valoracion;
            experiencia.Comentario = input.Comentario;

            await _experienciaRepository.UpdateAsync(experiencia);

            // Retornamos DTO simple
            return new ExperienciaDto
            {
                Id = experiencia.Id,
                DestinoId = experiencia.DestinoId,
                Valoracion = experiencia.Valoracion,
                Comentario = experiencia.Comentario,
                CreationTime = experiencia.CreationTime,
                UserName = CurrentUser.UserName
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var experiencia = await _experienciaRepository.GetAsync(id);

            // Validación: Solo el dueño puede borrar
            if (experiencia.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para eliminar esta experiencia.");
            }

            await _experienciaRepository.DeleteAsync(experiencia);
        }

        // 4.4, 4.5, 4.6 -> CONSULTA POTENTE
        public async Task<List<ExperienciaDto>> GetListAsync(Guid destinoId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null)
        {
            // 1. Obtenemos la consulta base (sin ejecutar aún en BD)
            var query = await _experienciaRepository.GetQueryableAsync();

            // 2. Filtramos por Destino (Obligatorio)
            query = query.Where(x => x.DestinoId == destinoId);

            // 3. Filtro por Valoración (Opcional - Punto 4.5)
            if (filtroValoracion.HasValue)
            {
                query = query.Where(x => x.Valoracion == filtroValoracion.Value);
            }

            // 4. Filtro por Texto / Palabras Clave (Opcional - Punto 4.6)
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                // ToLower para que no importe mayúsculas/minúsculas
                query = query.Where(x => x.Comentario.ToLower().Contains(filtroTexto.ToLower()));
            }

            // 5. Ordenamos por las más recientes primero
            query = query.OrderByDescending(x => x.CreationTime);

            // 6. Ejecutamos la consulta
            var experiencias = await AsyncExecuter.ToListAsync(query);

            // 7. Obtenemos los nombres de usuario (para mostrar "Juan dijo...")
            // Optimizacion: Buscamos solo los IDs de usuarios necesarios
            var userIds = experiencias.Select(x => x.CreatorId).Distinct().Where(id => id.HasValue).Select(id => id.Value).ToList();
            var usuarios = await _userRepository.GetListByIdsAsync(userIds);
            var diccionarioUsuarios = usuarios.ToDictionary(u => u.Id, u => u.UserName);

            // 8. Mapeamos a DTO
            return experiencias.Select(e => new ExperienciaDto
            {
                Id = e.Id,
                DestinoId = e.DestinoId,
                Valoracion = e.Valoracion,
                Comentario = e.Comentario,
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                UserName = e.CreatorId.HasValue && diccionarioUsuarios.ContainsKey(e.CreatorId.Value)
                           ? diccionarioUsuarios[e.CreatorId.Value]
                           : "Usuario Desconocido"
            }).ToList();
        }
    }
}