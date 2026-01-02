using AmmenTravel.Destinos;
using AmmenTravel.Opiniones.OpinionesDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using AmmenTravel.ListaFavoritos; // <--- NECESARIO PARA LAS ENTIDADES DE FAVORITOS

namespace AmmenTravel.Opiniones
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class OpinionAppService : ApplicationService, IOpinionAppService
    {
        private readonly ICreateUpdateOpinion _crearOpinionService;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly ICurrentUser _currentUser;

        // --- NUEVOS REPOSITORIOS ---
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritoRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritoRepository;

        public OpinionAppService(
            ICreateUpdateOpinion crearOpinionService,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<IdentityUser, Guid> userRepository,
            ICurrentUser currentUser,
            // Inyección de dependencias
            IRepository<ListaFavorito, Guid> listaFavoritoRepository,
            IRepository<LineaListaFavorito, Guid> lineaFavoritoRepository)
        {
            _crearOpinionService = crearOpinionService;
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _listaFavoritoRepository = listaFavoritoRepository;
            _lineaFavoritoRepository = lineaFavoritoRepository;
        }

        [HttpPost("api/app/opinion/crear-opinion")]
        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            return await _crearOpinionService.CrearOpinionAsync(input);
        }

        [HttpPut("api/app/opinion/actualizar-opinion/{opinionId}")]
        public async Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input)
        {
            return await _crearOpinionService.ActualizarOpinionAsync(opinionId, input);
        }

        [HttpDelete("api/app/opinion/eliminar-opinion/{opinionId}")]
        public async Task EliminarOpinionAsync(Guid opinionId)
        {
            await _crearOpinionService.EliminarOpinionAsync(opinionId);
        }

        [HttpPost("api/app/opinion/obtener-por-usuario/{usuarioId}")]
        public async Task<List<OpinionDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            if (!_currentUser.IsAuthenticated) throw new AbpAuthorizationException("Debe estar autenticado.");
            if (_currentUser.Id != usuarioId) throw new AbpAuthorizationException("No tiene permiso.");

            // 1. Obtener opiniones
            var opiniones = await _opinionRepository.GetListAsync(o => o.UserId == usuarioId);

            // 2. Obtener destinos relacionados
            var destinoIds = opiniones.Select(o => o.DestinoTuristicoId).Distinct().ToList();
            var destinos = await _destinoRepository.GetListAsync(d => destinoIds.Contains(d.Id));

            // 3. --- LOGICA DE FAVORITOS ---
            // Buscamos la lista del usuario
            var listaFavorito = await _listaFavoritoRepository.FirstOrDefaultAsync(l => l.UserId == usuarioId);
            var idsEnFavoritos = new HashSet<Guid>(); // HashSet para búsqueda rápida

            if (listaFavorito != null)
            {
                // Buscamos las líneas (destinos) de esa lista
                var lineas = await _lineaFavoritoRepository.GetListAsync(l => l.ListaFavoritoId == listaFavorito.Id);
                foreach (var linea in lineas)
                {
                    idsEnFavoritos.Add(linea.DestinoTuristicoId);
                }
            }

            // 4. Mapeo final
            return opiniones.Select(o =>
            {
                var destino = destinos.FirstOrDefault(d => d.Id == o.DestinoTuristicoId);
                return new OpinionDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    DestinoTuristicoId = o.DestinoTuristicoId,
                    NombreDestino = destino?.Nombre ?? "Destino no encontrado",
                    Comentario = o.Comentario,
                    Puntuacion = o.Puntuacion,
                    CreationTime = o.CreationTime,
                    IsDeleted = o.IsDeleted,
                    // Asignamos true si el destino está en el HashSet de favoritos
                    EsFavorito = idsEnFavoritos.Contains(o.DestinoTuristicoId)
                };
            }).ToList();
        }

        [HttpPost("api/app/opinion/obtener-lista-publica-por-destino")]
        public async Task<List<OpinionPublicaDto>> ObtenerListaPublicaPorDestinoAsync(string idExternoGeoDB)
        {
            // (Código original sin cambios)
            var destino = await _destinoRepository.FirstOrDefaultAsync(d => d.IdExterno == idExternoGeoDB);
            if (destino == null) return new List<OpinionPublicaDto>();

            var queryable = await _opinionRepository.GetQueryableAsync();
            var opiniones = await queryable.IgnoreQueryFilters().Where(o => o.DestinoTuristicoId == destino.Id).OrderByDescending(o => o.CreationTime).ToListAsync();
            if (!opiniones.Any()) return new List<OpinionPublicaDto>();

            var userIds = opiniones.Select(o => o.UserId).Distinct().ToList();
            var usuarios = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));

            return opiniones.Select(op => {
                var usuario = usuarios.FirstOrDefault(u => u.Id == op.UserId);
                return new OpinionPublicaDto
                {
                    Id = op.Id,
                    NombreUsuario = usuario?.Name ?? usuario?.UserName ?? "Usuario Anónimo",
                    Puntuacion = (int)op.Puntuacion,
                    Comentario = op.Comentario,
                    CreationTime = op.CreationTime
                };
            }).ToList();
        }

        // Angular llama a: /api/app/opinion/obtener-lista-publica-por-destino (POST)

        [HttpPost("api/app/opinion/es-opinionado/{destinoId}")]
        public async Task<bool> EsOpinionadoAsync(Guid destinoId)
        {
            // (Código original sin cambios)
            if (!_currentUser.IsAuthenticated) return false;
            var userId = _currentUser.Id;
            if (!userId.HasValue) return false;
            var opinionExistente = await _opinionRepository.FirstOrDefaultAsync(o => o.DestinoTuristicoId == destinoId && o.UserId == userId.Value);
            return opinionExistente != null;
        }


    }

}

