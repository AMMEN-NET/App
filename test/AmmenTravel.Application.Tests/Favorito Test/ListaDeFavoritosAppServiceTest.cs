using AmmenTravel.ListaDeFavoritos;
using AmmenTravel.ListaDeFavoritos;
using AmmenTravel.ListaFavoritos;
using NSubstitute;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Xunit;

namespace AmmenTravel.Favorito_Test
{
    public abstract class ListaDeFavoritosAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IRepository<ListaFavorito, Guid> _listaRepo;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaRepo;
        private readonly ICurrentUser _currentUser;
        private readonly ListaDeFavoritosAppService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public ListaDeFavoritosAppServiceTest()
        {
            _listaRepo = Substitute.For<IRepository<ListaFavorito, Guid>>();
            _lineaRepo = Substitute.For<IRepository<LineaListaFavorito, Guid>>();
            _currentUser = Substitute.For<ICurrentUser>();
            _currentUser.IsAuthenticated.Returns(true);
            _currentUser.Id.Returns(_userId);

            _service = new ListaDeFavoritosAppService(_listaRepo, _lineaRepo, _currentUser);
        }

        [Fact]
        public async Task GetOrCreateListaAsync_ShouldThrow_WhenNotAuthenticated()
        {
            _currentUser.IsAuthenticated.Returns(false);

            await Should.ThrowAsync<AbpAuthorizationException>(async () =>
            {
                await _service.GetOrCreateListaAsync();
            });
        }

        [Fact]
        public async Task GetOrCreateListaAsync_ShouldCreate_WhenNotExists()
        {
            // Arrange: lista no existente
            _listaRepo
                .FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult<ListaFavorito>(null));

            // Devuelve la misma instancia pasada al InsertAsync (sin intentar setear Id)
            _listaRepo
                .InsertAsync(Arg.Any<ListaFavorito>())
                .Returns(ci => Task.FromResult(ci.ArgAt<ListaFavorito>(0)));

            // Act
            var listaResult = await _service.GetOrCreateListaAsync();

            // Assert
            listaResult.ShouldNotBeNull();
            listaResult.UserId.ShouldBe(_userId);
            await _listaRepo.Received(1).InsertAsync(Arg.Is<ListaFavorito>(l => l.UserId == _userId));
        }

        [Fact]
        public async Task GetOrCreateListaAsync_ShouldReturnExisting_WhenExists()
        {
            var existing = new ListaFavorito { UserId = _userId }; // no set de Id

            _listaRepo
                .FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(existing));

            var result = await _service.GetOrCreateListaAsync();

            result.ShouldNotBeNull();
            result.ShouldBe(existing);
            await _listaRepo.DidNotReceive().InsertAsync(Arg.Any<ListaFavorito>());
        }

        [Fact]
        public async Task AgregarAFavoritosAsync_ShouldInsert_WhenNotExists()
        {
            var lista = new ListaFavorito { UserId = _userId }; // no set de Id
            var destinoId = Guid.NewGuid();

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult<LineaListaFavorito>(null));

            await _service.AgregarAFavoritosAsync(destinoId);

            await _lineaRepo.Received(1).InsertAsync(Arg.Is<LineaListaFavorito>(l =>
                l.ListaFavoritoId == lista.Id && l.DestinoTuristicoId == destinoId));
        }

        [Fact]
        public async Task AgregarAFavoritosAsync_ShouldNotInsert_WhenAlreadyExists()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var destinoId = Guid.NewGuid();
            var existingLinea = new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = destinoId };

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult(existingLinea));

            await _service.AgregarAFavoritosAsync(destinoId);

            await _lineaRepo.DidNotReceive().InsertAsync(Arg.Any<LineaListaFavorito>());
        }

        [Fact]
        public async Task EliminarDeFavoritosAsync_ShouldDelete_WhenExists()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var destinoId = Guid.NewGuid();
            var linea = new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = destinoId };

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult(linea));

            await _service.EliminarDeFavoritosAsync(destinoId);

            await _lineaRepo.Received(1).DeleteAsync(Arg.Is<LineaListaFavorito>(l => l == linea));
        }

        [Fact]
        public async Task EliminarDeFavoritosAsync_ShouldNotDelete_WhenNotExists()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var destinoId = Guid.NewGuid();

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult<LineaListaFavorito>(null));

            await _service.EliminarDeFavoritosAsync(destinoId);

            await _lineaRepo.DidNotReceive().DeleteAsync(Arg.Any<LineaListaFavorito>());
        }

        [Fact]
        public async Task ObtenerFavoritosAsync_ShouldReturn_AllDestinoIds()    // A chequear esto, no sé que tan bien estará
        {
            var lista = new ListaFavorito { UserId = _userId };
            var destinos = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var lineas = destinos.Select(d => new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = d }).ToList();

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            // CORRECCIÓN QUE SE HIZO:
            // 1. Se usó Arg.Any<bool> y CancellationToken para coincidir con la firma.
            // 2. Se sacó el (IList<LineaListaFavorito>) dentro del Task.FromResult.

            _lineaRepo.GetListAsync(
                    Arg.Any<Expression<Func<LineaListaFavorito, bool>>>(),
                    Arg.Any<bool>(),
                    Arg.Any<System.Threading.CancellationToken>()
                )
                .Returns(Task.FromResult(lineas));

            var result = await _service.ObtenerFavoritosAsync();

            result.ShouldBeEquivalentTo(destinos);
        }

        [Fact]
        public async Task EsFavoritoAsync_ShouldReturnTrue_WhenExists_AndFalse_WhenNot()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var destinoExistente = Guid.NewGuid();
            var destinoNoExistente = Guid.NewGuid();

            var linea = new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = destinoExistente };

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            // Primera llamada: devuelve la línea existente
            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult<LineaListaFavorito>(linea));

            var esFav = await _service.EsFavoritoAsync(destinoExistente);
            esFav.ShouldBeTrue();

            // Ahora simular ausencia
            _lineaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult<LineaListaFavorito>(null));

            var noEsFav = await _service.EsFavoritoAsync(destinoNoExistente);
            noEsFav.ShouldBeFalse();
        }

        /*                                                                              HAY QUE REVISAR ESTO
        [Fact]
        public async Task VaciarFavoritosAsync_ShouldDeleteAllLineas()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var lineas = new List<LineaListaFavorito>
            {
                new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = Guid.NewGuid() },
                new LineaListaFavorito { ListaFavoritoId = lista.Id, DestinoTuristicoId = Guid.NewGuid() },
            };

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.GetListAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult((IList<LineaListaFavorito>)lineas));

            await _service.VaciarFavoritosAsync();

            foreach (var linea in lineas)
            {
                await _lineaRepo.Received(1).DeleteAsync(Arg.Is<LineaListaFavorito>(l => l == linea));
            }
        } 
        */

        [Fact]
        public async Task ContarFavoritosAsync_ShouldReturnCountFromRepository()
        {
            var lista = new ListaFavorito { UserId = _userId };
            var expectedCount = 5;

            _listaRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<ListaFavorito, bool>>>())
                .Returns(Task.FromResult(lista));

            _lineaRepo.CountAsync(Arg.Any<Expression<Func<LineaListaFavorito, bool>>>())
                .Returns(Task.FromResult(expectedCount));

            var result = await _service.ContarFavoritosAsync();

            result.ShouldBe(expectedCount);
        }
    }
}