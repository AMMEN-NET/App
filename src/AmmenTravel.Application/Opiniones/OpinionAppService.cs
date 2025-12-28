using AmmenTravel.Destinos;
using AmmenTravel.Opiniones.OpinionesDTO;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<OpinionDto> CrearOpinionAsync(createUpdateOpinionDto input)
        {
            return await _crearOpinionService.CrearOpinionAsync(input);
        }

        public async Task<OpinionDto> ActualizarOpinionAsync(Guid opinionId, createUpdateOpinionDto input)
        {
            return await _crearOpinionService.ActualizarOpinionAsync(opinionId, input);
        }

        public async Task EliminarOpinionAsync(Guid opinionId)
        {
            await _crearOpinionService.EliminarOpinionAsync(opinionId);
        }

        public async Task<List<OpinionDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            if (!_currentUser.IsAuthenticated) throw new AbpAuthorizationException("Debe estar autenticado.");
            if (_currentUser.Id != usuarioId) throw new AbpAuthorizationException("No tiene permiso.");

            var opiniones = await _opinionRepository.GetListAsync(o => o.UserId == usuarioId);

            // Obtenemos todos los IDs de destinos para hacer una sola consulta
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

        public async Task<List<OpinionPublicaDto>> ObtenerListaPublicaPorDestinoAsync(string idExternoGeoDB)
        {
            // 1. Buscamos si el destino existe en nuestra BD interna usando el ID de GeoDB
            var destino = await _destinoRepository.FirstOrDefaultAsync(d => d.IdExterno == idExternoGeoDB);

            if (destino == null)
            {
                return new List<OpinionPublicaDto>();
            }

            // 2. Obtenemos el Queryable de opiniones
            var queryable = await _opinionRepository.GetQueryableAsync();

            // 3. Usamos IgnoreQueryFilters para ver las opiniones de OTROS usuarios
            var opiniones = await queryable
                .IgnoreQueryFilters()
                .Where(o => o.DestinoTuristicoId == destino.Id)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync();

            if (!opiniones.Any())
            {
                return new List<OpinionPublicaDto>();
            }

            // 4. Obtenemos los IDs de los usuarios que opinaron
            var userIds = opiniones.Select(o => o.UserId).Distinct().ToList();
            var usuarios = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));

            // 5. Mapeamos a DTO combinando la info
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