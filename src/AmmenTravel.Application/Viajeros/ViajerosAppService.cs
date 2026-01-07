using System;
using System.Linq;
using System.Threading.Tasks;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore; // IMPORTANTE para IgnoreQueryFilters y CountAsync
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace AmmenTravel.Viajeros
{
    [Authorize]
    public class ViajerosAppService : AmmenTravelAppService, IViajerosAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritoRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritoRepository;

        public ViajerosAppService(
            IIdentityUserRepository userRepository,
            IRepository<Opinion, Guid> opinionRepository,
            IRepository<ListaFavorito, Guid> listaFavoritoRepository,
            IRepository<LineaListaFavorito, Guid> lineaFavoritoRepository)
        {
            _userRepository = userRepository;
            _opinionRepository = opinionRepository;
            _listaFavoritoRepository = listaFavoritoRepository;
            _lineaFavoritoRepository = lineaFavoritoRepository;
        }

        public async Task<System.Collections.Generic.List<ViajeroDto>> GetListAsync(string filtro = null)
        {
            var usuarios = await _userRepository.GetListAsync(
                sorting: "UserName",
                maxResultCount: 50,
                filter: filtro
            );

            return usuarios.Select(u => new ViajeroDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                Surname = u.Surname
            }).ToList();
        }

        public async Task<PerfilPublicoDto> GetPerfilPublicoAsync(Guid id)
        {
            // 1. Obtener Usuario
            var usuario = await _userRepository.GetAsync(id);

            // 2. Contar Opiniones (Ignorando filtros para ver las de OTROS usuarios)
            var queryOpiniones = await _opinionRepository.GetQueryableAsync();
            var cantidadOpiniones = await queryOpiniones
                .IgnoreQueryFilters()
                .CountAsync(x => x.UserId == id && !x.IsDeleted); // Importante: !IsDeleted manual si ignoramos filtros

            // 3. Contar Favoritos (Lógica corregida: Contar LÍNEAS, no listas)
            var queryListas = await _listaFavoritoRepository.GetQueryableAsync();

            // Buscamos la lista de ese usuario específico
            var listaUsuario = await queryListas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.UserId == id && !x.IsDeleted);

            int cantidadFavoritos = 0;
            if (listaUsuario != null)
            {
                var queryLineas = await _lineaFavoritoRepository.GetQueryableAsync();
                cantidadFavoritos = await queryLineas
                    .IgnoreQueryFilters()
                    .CountAsync(x => x.ListaFavoritoId == listaUsuario.Id);
            }

            // 4. Devolver DTO
            return new PerfilPublicoDto
            {
                Id = usuario.Id,
                UserName = usuario.UserName,
                Name = usuario.Name,
                Surname = usuario.Surname,
                FechaRegistro = usuario.CreationTime,

                CantidadOpiniones = cantidadOpiniones,
                CantidadFavoritos = cantidadFavoritos,
                PromedioPuntuacion = 0 // Pendiente de implementar si lo deseas
            };
        }
    }
}