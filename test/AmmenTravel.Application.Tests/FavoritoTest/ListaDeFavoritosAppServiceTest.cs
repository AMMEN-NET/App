using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmmenTravel.ListaFavoritos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;
using AmmenTravel.InterfaceDestinoAppService;
using AmmenTravel.ExternalService;
using AmmenTravel.Destinos;
using AmmenTravel.Favoritos.FavoritosDTO;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query; // <-- añadido para IAsyncQueryProvider

namespace AmmenTravel.ListaFavoritos
{
    [Authorize]
    public class ListaDeFavoritosAppServiceTest : ApplicationService
    {
        private readonly IRepository<ListaFavorito, Guid> _listaRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IDestinoAppService _destinoAppService;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;

        public ListaDeFavoritosAppServiceTest(
            IRepository<ListaFavorito, Guid> listaRepository,
            IRepository<LineaListaFavorito, Guid> lineaRepository,
            ICurrentUser currentUser,
            IDestinoAppService destinoAppService,
            IRepository<DestinoTuristico, Guid> destinoRepository,
            IRepository<Opinion, Guid> opinionRepository)
        {
            _listaRepository = listaRepository;
            _lineaRepository = lineaRepository;
            _currentUser = currentUser;
            _destinoAppService = destinoAppService;
            _destinoRepository = destinoRepository;
            _opinionRepository = opinionRepository;
        }

        public async Task<ListaFavorito> GetOrCreateListaAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("Debe estar autenticado para gestionar favoritos.");
            }

            var userId = _currentUser.Id.Value;
            var lista = await _listaRepository.FirstOrDefaultAsync(l => l.UserId == userId);
            if (lista == null)
            {
                lista = new ListaFavorito
                {
                    UserId = userId
                };
                await _listaRepository.InsertAsync(lista);
            }

            return lista;
        }

        public async Task AgregarAFavoritosAsync(Guid destinoId)
        {
            var lista = await GetOrCreateListaAsync();

            var existe = await _lineaRepository.FirstOrDefaultAsync(l =>
                l.ListaFavoritoId == lista.Id && l.DestinoTuristicoId == destinoId);

            if (existe == null)
            {
                var linea = new LineaListaFavorito
                {
                    ListaFavoritoId = lista.Id,
                    DestinoTuristicoId = destinoId
                };
                await _lineaRepository.InsertAsync(linea);
            }
        }

        public async Task AgregarFavoritoDesdeBusquedaAsync(CiudadDTO ciudadExterna)
        {
            var destinoId = await _destinoAppService.BuscarOCrearDestinoDesdeApiAsync(ciudadExterna);
            await AgregarAFavoritosAsync(destinoId);
        }

        public async Task EliminarDeFavoritosAsync(Guid destinoId)
        {
            var lista = await GetOrCreateListaAsync();
            var linea = await _lineaRepository.FirstOrDefaultAsync(l =>
                l.ListaFavoritoId == lista.Id && l.DestinoTuristicoId == destinoId);

            if (linea != null)
            {
                await _lineaRepository.DeleteAsync(linea);
            }
        }

        public async Task<List<FavoritoDto>> ObtenerFavoritosAsync()
        {
            var lista = await GetOrCreateListaAsync();
            var lineas = await _lineaRepository.GetListAsync(l => l.ListaFavoritoId == lista.Id);

            if (!lineas.Any()) return new List<FavoritoDto>();

            var destinoIds = lineas.Select(l => l.DestinoTuristicoId).ToList();
            var destinos = await _destinoRepository.GetListAsync(d => destinoIds.Contains(d.Id));

            // 1. Obtenemos el Queryable para ignorar filtros automáticos de usuario (IUserOwned)
            var queryable = await _opinionRepository.GetQueryableAsync();

            // 2. Construimos la consulta filtrada
            var filtered = queryable
                .IgnoreQueryFilters()
                .Where(o => destinoIds.Contains(o.DestinoTuristicoId) && !o.IsDeleted);

            // 3. Ejecutamos de forma asíncrona sólo si el provider lo soporta (p. ej. EF Core),
            //    en tests mocks simples el provider no será IAsyncQueryProvider y cae al ToList() sincrono.
            List<Opinion> opiniones;
            if (filtered.Provider is IAsyncQueryProvider)
            {
                opiniones = await filtered.ToListAsync();
            }
            else
            {
                opiniones = filtered.ToList();
            }

            return destinos.Select(d =>
            {
                var opinionesDestino = opiniones.Where(o => o.DestinoTuristicoId == d.Id).ToList();
                double? promedio = null;
                if (opinionesDestino.Any())
                {
                    promedio = opinionesDestino.Average(o => (int)o.Puntuacion);
                }

                return new FavoritoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Pais = d.Pais,
                    Poblacion = d.Poblacion,
                    Latitud = d.Latitud,
                    Longitud = d.Longitud,
                    GeoDBId = d.IdExterno ?? "",

                    CantidadOpiniones = opinionesDestino.Count,
                    PromedioPuntuacion = promedio
                };
            }).ToList();
        }

        public async Task<bool> EsFavoritoAsync(Guid destinoId)
        {
            var lista = await GetOrCreateListaAsync();
            var existe = await _lineaRepository.FirstOrDefaultAsync(l =>
                l.ListaFavoritoId == lista.Id && l.DestinoTuristicoId == destinoId);

            return existe != null;
        }

        public async Task VaciarFavoritosAsync()
        {
            var lista = await GetOrCreateListaAsync();
            var lineas = await _lineaRepository.GetListAsync(l => l.ListaFavoritoId == lista.Id);

            foreach (var linea in lineas)
            {
                await _lineaRepository.DeleteAsync(linea);
            }
        }

        public async Task<int> ContarFavoritosAsync()
        {
            var lista = await GetOrCreateListaAsync();
            return await _lineaRepository.CountAsync(l => l.ListaFavoritoId == lista.Id);
        }
    }
}