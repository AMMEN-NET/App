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

        public OpinionAppService(
            ICreateUpdateOpinion crearOpinionService,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<IdentityUser, Guid> userRepository,
            ICurrentUser currentUser)
        {
            _crearOpinionService = crearOpinionService;
            _opinionRepository = opinionRepository;
            _destinoRepository = destinoRepository;
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        // Angular llama a: /api/app/opinion/crear-opinion (POST)
        // Por defecto es POST, pero es buena práctica hacerlo explícito si cambias nombres
        [HttpPost("api/app/opinion/crear-opinion")]
        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            return await _crearOpinionService.CrearOpinionAsync(input);
        }

        // Angular llama a: /api/app/opinion/actualizar-opinion/{opinionId} (PUT)
        // Corrección: Forzar PUT y definir la ruta con el parámetro
        [HttpPut("api/app/opinion/actualizar-opinion/{opinionId}")]
        public async Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input)
        {
            return await _crearOpinionService.ActualizarOpinionAsync(opinionId, input);
        }

        // Angular llama a: /api/app/opinion/eliminar-opinion/{opinionId} (DELETE)
        // Corrección: Forzar DELETE y definir la ruta
        [HttpDelete("api/app/opinion/eliminar-opinion/{opinionId}")]
        public async Task EliminarOpinionAsync(Guid opinionId)
        {
            await _crearOpinionService.EliminarOpinionAsync(opinionId);
        }

        // Angular llama a: /api/app/opinion/obtener-por-usuario/{usuarioId} (POST)
        // "Obtener" no es "Get" para ABP, así que por defecto es POST. Esto ya coincidía con Angular.
        [HttpPost("api/app/opinion/obtener-por-usuario/{usuarioId}")]
        public async Task<List<OpinionDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            if (!_currentUser.IsAuthenticated) throw new AbpAuthorizationException("Debe estar autenticado.");
            if (_currentUser.Id != usuarioId) throw new AbpAuthorizationException("No tiene permiso.");

            var opiniones = await _opinionRepository.GetListAsync(o => o.UserId == usuarioId);

            var destinoIds = opiniones.Select(o => o.DestinoTuristicoId).Distinct().ToList();
            var destinos = await _destinoRepository.GetListAsync(d => destinoIds.Contains(d.Id));

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
                    CreationTime = o.CreationTime
                };
            }).ToList();
        }

        // Angular llama a: /api/app/opinion/obtener-lista-publica-por-destino (POST)
        [HttpPost("api/app/opinion/obtener-lista-publica-por-destino")]
        public async Task<List<OpinionPublicaDto>> ObtenerListaPublicaPorDestinoAsync(string idExternoGeoDB)
        {
            var destino = await _destinoRepository.FirstOrDefaultAsync(d => d.IdExterno == idExternoGeoDB);

            if (destino == null)
            {
                return new List<OpinionPublicaDto>();
            }

            var queryable = await _opinionRepository.GetQueryableAsync();

            var opiniones = await queryable
                .IgnoreQueryFilters()
                .Where(o => o.DestinoTuristicoId == destino.Id)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync();

            if (!opiniones.Any())
            {
                return new List<OpinionPublicaDto>();
            }

            var userIds = opiniones.Select(o => o.UserId).Distinct().ToList();
            var usuarios = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));

            var resultado = opiniones.Select(op =>
            {
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

            return resultado;
        }

        // Angular llama a: /api/app/opinion/es-opinionado/{destinoId} (POST)
        [HttpPost("api/app/opinion/es-opinionado/{destinoId}")]
        public async Task<bool> EsOpinionadoAsync(Guid destinoId)
        {
            if (!_currentUser.IsAuthenticated)
                return false;

            var userId = _currentUser.Id;
            if (!userId.HasValue)
                return false;

            var opinionExistente = await _opinionRepository.FirstOrDefaultAsync(
                o => o.DestinoTuristicoId == destinoId && o.UserId == userId.Value);

            return opinionExistente != null;
        }
    }
}