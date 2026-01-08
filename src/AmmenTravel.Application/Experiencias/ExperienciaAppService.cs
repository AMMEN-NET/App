using AmmenTravel.Destinos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // Necesario para .Include() e .IgnoreQueryFilters()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace AmmenTravel.Experiencias
{
    [Authorize]
    public class ExperienciaAppService : AmmenTravelAppService, IExperienciaAppService
    {
        private readonly IRepository<Experiencia, Guid> _experienciaRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;

        public ExperienciaAppService(
            IRepository<Experiencia, Guid> experienciaRepository,
            IIdentityUserRepository userRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository)
        {
            _experienciaRepository = experienciaRepository;
            _userRepository = userRepository;
            _destinoRepository = destinoRepository;
        }

        public async Task<ExperienciaDto> CreateAsync(CreateUpdateExperienciaDto input)
        {
            var experiencia = new Experiencia(
                GuidGenerator.Create(),
                input.DestinoId,
                input.Valoracion,
                input.Comentario
            );

            await _experienciaRepository.InsertAsync(experiencia);

            // Cargamos el nombre del destino para devolver el DTO completo
            var destino = await _destinoRepository.GetAsync(input.DestinoId);
            var usuarioActual = await _userRepository.GetAsync(CurrentUser.Id.Value);

            return new ExperienciaDto
            {
                Id = experiencia.Id,
                DestinoId = experiencia.DestinoId,
                DestinoNombre = destino.Nombre,
                Valoracion = experiencia.Valoracion,
                Comentario = experiencia.Comentario,
                CreationTime = DateTime.Now,
                CreatorId = usuarioActual.Id,
                UserName = usuarioActual.UserName
            };
        }

        public async Task<ExperienciaDto> UpdateAsync(Guid id, CreateUpdateExperienciaDto input)
        {
            var queryable = await _experienciaRepository.GetQueryableAsync();
            var experiencia = await queryable
                .Include(x => x.Destino)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (experiencia == null) throw new EntityNotFoundException(typeof(Experiencia), id);

            if (experiencia.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para editar esta experiencia.");
            }

            experiencia.Valoracion = input.Valoracion;
            experiencia.Comentario = input.Comentario;

            await _experienciaRepository.UpdateAsync(experiencia);

            return new ExperienciaDto
            {
                Id = experiencia.Id,
                DestinoId = experiencia.DestinoId,
                DestinoNombre = experiencia.Destino?.Nombre,
                Valoracion = experiencia.Valoracion,
                Comentario = experiencia.Comentario,
                CreationTime = experiencia.CreationTime,
                UserName = CurrentUser.UserName
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var experiencia = await _experienciaRepository.GetAsync(id);

            if (experiencia.CreatorId != CurrentUser.Id)
            {
                throw new UserFriendlyException("No tienes permiso para eliminar esta experiencia.");
            }

            await _experienciaRepository.DeleteAsync(experiencia);
        }

        // Método para ver experiencias PÚBLICAS (de un destino o todas)
        public async Task<List<ExperienciaDto>> GetListAsync(string? destinoId = null, TipoExperiencia? filtroValoracion = null, string filtroTexto = null)
        {
            Guid? idReal = null;

            // 1. Resolver ID solo si se envía uno (Guid o Externo)
            if (!string.IsNullOrWhiteSpace(destinoId))
            {
                if (Guid.TryParse(destinoId, out var parsedGuid))
                {
                    idReal = parsedGuid;
                }
                else
                {
                    var destino = await _destinoRepository.FirstOrDefaultAsync(d => d.IdExterno == destinoId);
                    if (destino != null)
                    {
                        idReal = destino.Id;
                    }
                }
            }

            // 2. Construir Query Robusta
            var queryable = await _experienciaRepository.GetQueryableAsync();

            var query = queryable
                .IgnoreQueryFilters()       // CLAVE: Ignora filtros de seguridad (IUserOwned) y SoftDelete
                .Include(x => x.Destino)    // JOIN Explícito
                .Where(x => !x.IsDeleted);  // Re-aplicamos filtro de borrado lógico manualmente

            // 3. Aplicar Filtro de Destino (Solo si se resolvió un ID válido)
            if (idReal.HasValue)
            {
                query = query.Where(x => x.DestinoId == idReal.Value);
            }

            // 4. Filtros opcionales
            if (filtroValoracion.HasValue)
            {
                query = query.Where(x => x.Valoracion == filtroValoracion.Value);
            }

            // NUEVO: El buscador busca en el comentario O en el nombre del destino
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                var texto = filtroTexto.ToLower();
                query = query.Where(x =>
                    x.Comentario.ToLower().Contains(texto) ||
                    x.Destino.Nombre.ToLower().Contains(texto));
            }

            query = query.OrderByDescending(x => x.CreationTime);

            var experiencias = await AsyncExecuter.ToListAsync(query);

            // 5. Mapeo de Usuarios
            var userIds = experiencias.Select(x => x.CreatorId).Distinct().Where(id => id.HasValue).Select(id => id.Value).ToList();
            var usuarios = await _userRepository.GetListByIdsAsync(userIds);
            var diccionarioUsuarios = usuarios.ToDictionary(u => u.Id, u => u.UserName);

            return experiencias.Select(e => new ExperienciaDto
            {
                Id = e.Id,
                DestinoId = e.DestinoId,
                DestinoNombre = e.Destino?.Nombre ?? "Destino Desconocido",
                Valoracion = e.Valoracion,
                Comentario = e.Comentario,
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                UserName = e.CreatorId.HasValue && diccionarioUsuarios.ContainsKey(e.CreatorId.Value)
                            ? diccionarioUsuarios[e.CreatorId.Value]
                            : "Usuario Desconocido"
            }).ToList();
        }

        // Método para ver "MIS EXPERIENCIAS" (Perfil de usuario)
        public async Task<List<ExperienciaDto>> GetListPorUsuarioAsync(Guid userId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null)
        {
            // 1. Construir Query Robusta
            var queryable = await _experienciaRepository.GetQueryableAsync();

            var query = queryable
                .Include(x => x.Destino) // JOIN Explícito
                .Where(x => x.CreatorId == userId);

            // 2. Filtros opcionales
            if (filtroValoracion.HasValue)
            {
                query = query.Where(x => x.Valoracion == filtroValoracion.Value);
            }

            // NUEVO: Búsqueda también por nombre de destino en Mis Experiencias
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                var texto = filtroTexto.ToLower();
                query = query.Where(x =>
                    x.Comentario.ToLower().Contains(texto) ||
                    x.Destino.Nombre.ToLower().Contains(texto));
            }

            query = query.OrderByDescending(x => x.CreationTime);

            var experiencias = await AsyncExecuter.ToListAsync(query);

            // 3. Obtener nombre del usuario
            var usuario = await _userRepository.GetAsync(userId);
            string nombreUsuario = usuario.UserName;

            // 4. Mapeo
            return experiencias.Select(e => new ExperienciaDto
            {
                Id = e.Id,
                DestinoId = e.DestinoId,
                DestinoNombre = e.Destino?.Nombre ?? "Destino Eliminado",
                Valoracion = e.Valoracion,
                Comentario = e.Comentario,
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                UserName = nombreUsuario
            }).ToList();
        }
    }
}