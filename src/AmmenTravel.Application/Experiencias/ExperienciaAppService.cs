using AmmenTravel.Destinos;
using Microsoft.AspNetCore.Authorization;
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
                DestinoNombre = destino.Nombre, // Devolvemos el nombre
                Valoracion = experiencia.Valoracion,
                Comentario = experiencia.Comentario,
                CreationTime = DateTime.Now,
                CreatorId = usuarioActual.Id,
                UserName = usuarioActual.UserName
            };
        }

        public async Task<ExperienciaDto> UpdateAsync(Guid id, CreateUpdateExperienciaDto input)
        {
            // Usamos WithDetailsAsync para traer la relación del destino
            var query = await _experienciaRepository.WithDetailsAsync(x => x.Destino);
            var experiencia = query.FirstOrDefault(x => x.Id == id);

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

        public async Task<List<ExperienciaDto>> GetListAsync(string destinoId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null)
        {
            Guid idReal;

            // 1. Resolver ID (Guid o Externo)
            if (Guid.TryParse(destinoId, out var parsedGuid))
            {
                idReal = parsedGuid;
            }
            else
            {
                var destino = await _destinoRepository.FirstOrDefaultAsync(d => d.IdExterno == destinoId);
                if (destino == null) return new List<ExperienciaDto>();
                idReal = destino.Id;
            }

            // 2. OBTENER QUERY CON INCLUDES (ESTO EVITA QUE EXPLOTE AL BUSCAR EL NOMBRE)
            // 'WithDetailsAsync' le dice a EF Core que haga el JOIN con la tabla Destinos
            var query = await _experienciaRepository.WithDetailsAsync(x => x.Destino);

            // 3. Aplicar Filtros sobre la Query en memoria o IQueryable
            // Nota: Al usar WithDetailsAsync a veces devuelve IQueryable o List dependiendo de la versión de ABP.
            // Para asegurar, trabajamos sobre la queryable:

            var queryFiltrada = query.AsQueryable().Where(x => x.DestinoId == idReal);

            if (filtroValoracion.HasValue)
            {
                queryFiltrada = queryFiltrada.Where(x => x.Valoracion == filtroValoracion.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                queryFiltrada = queryFiltrada.Where(x => x.Comentario.ToLower().Contains(filtroTexto.ToLower()));
            }

            queryFiltrada = queryFiltrada.OrderByDescending(x => x.CreationTime);

            var experiencias = await AsyncExecuter.ToListAsync(queryFiltrada);

            // 4. Obtener usuarios para mostrar nombres
            var userIds = experiencias.Select(x => x.CreatorId).Distinct().Where(id => id.HasValue).Select(id => id.Value).ToList();
            var usuarios = await _userRepository.GetListByIdsAsync(userIds);
            var diccionarioUsuarios = usuarios.ToDictionary(u => u.Id, u => u.UserName);

            // 5. Mapear a DTO
            return experiencias.Select(e => new ExperienciaDto
            {
                Id = e.Id,
                DestinoId = e.DestinoId,
                // Aquí asignamos el nombre de forma segura. Si Destino es null (error de integridad vieja), ponemos "Desconocido"
                DestinoNombre = e.Destino != null ? e.Destino.Nombre : "Destino Desconocido",
                Valoracion = e.Valoracion,
                Comentario = e.Comentario,
                CreationTime = e.CreationTime,
                CreatorId = e.CreatorId,
                UserName = e.CreatorId.HasValue && diccionarioUsuarios.ContainsKey(e.CreatorId.Value)
                            ? diccionarioUsuarios[e.CreatorId.Value]
                            : "Usuario Desconocido"
            }).ToList();
        }
        public async Task<List<ExperienciaDto>> GetListPorUsuarioAsync(Guid userId, TipoExperiencia? filtroValoracion = null, string filtroTexto = null)
{
    var query = await _experienciaRepository.WithDetailsAsync(x => x.Destino);

    // 2. Filtramos DIRECTO por el ID del usuario (CreatorId es campo de auditoría de ABP)
    var queryFiltrada = query.AsQueryable().Where(x => x.CreatorId == userId);

    // 3. Filtros opcionales (por si el usuario quiere buscar en sus propias reseñas)
    if (filtroValoracion.HasValue)
    {
        queryFiltrada = queryFiltrada.Where(x => x.Valoracion == filtroValoracion.Value);
    }

    if (!string.IsNullOrWhiteSpace(filtroTexto))
    {
        queryFiltrada = queryFiltrada.Where(x => x.Comentario.ToLower().Contains(filtroTexto.ToLower()));
    }

    // 4. Ordenamos por fecha (lo más nuevo arriba)
    queryFiltrada = queryFiltrada.OrderByDescending(x => x.CreationTime);

    // 5. Ejecutamos la consulta
    var experiencias = await AsyncExecuter.ToListAsync(queryFiltrada);

    // 6. Obtenemos el nombre del usuario UNA sola vez (ya que todas son del mismo userId)
    var usuario = await _userRepository.GetAsync(userId);
    string nombreUsuario = usuario.UserName;

    // 7. Mapeamos a DTO
    return experiencias.Select(e => new ExperienciaDto
    {
        Id = e.Id,
        DestinoId = e.DestinoId,
        
        // Acá es importante: mostramos el nombre del destino
        DestinoNombre = e.Destino != null ? e.Destino.Nombre : "Destino Eliminado",
        
        Valoracion = e.Valoracion,
        Comentario = e.Comentario,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        
        // Como todas son del mismo usuario, usamos el nombre que buscamos arriba
        UserName = nombreUsuario 
    }).ToList();
}
    } 
}