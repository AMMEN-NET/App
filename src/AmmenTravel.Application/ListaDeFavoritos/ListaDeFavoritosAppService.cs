using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmmenTravel.ListaFavoritos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp;
using Microsoft.AspNetCore.Authorization;

namespace AmmenTravel.ListaDeFavoritos
{
    [Authorize]
    public class ListaDeFavoritosAppService : ApplicationService
    {
        private readonly IRepository<ListaFavorito, Guid> _listaRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaRepository;
        private readonly ICurrentUser _currentUser;

        public ListaDeFavoritosAppService(
            IRepository<ListaFavorito, Guid> listaRepository,
            IRepository<LineaListaFavorito, Guid> lineaRepository,
            ICurrentUser currentUser)
        {
            _listaRepository = listaRepository;
            _lineaRepository = lineaRepository;
            _currentUser = currentUser;
        }

        // Crea o devuelve la lista de favoritos del usuario actual (lanza si no está autenticado)
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

        public async Task<IList<Guid>> ObtenerFavoritosAsync()
        {
            var lista = await GetOrCreateListaAsync();

            var lineas = await _lineaRepository.GetListAsync(l => l.ListaFavoritoId == lista.Id);

            return lineas.Select(l => l.DestinoTuristicoId).ToList();
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
