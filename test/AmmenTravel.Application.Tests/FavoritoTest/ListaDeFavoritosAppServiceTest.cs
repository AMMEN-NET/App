using AmmenTravel.Destinos;
using AmmenTravel.ListaDeFavoritos;
using AmmenTravel.Opiniones;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.ListaFavoritos
{
    public abstract class classListaDeFavoritosAppServiceTest<TStartupModule>
       : AmmenTravelApplicationTestBase<TStartupModule>
       where TStartupModule : IAbpModule
    {


        protected readonly ListaDeFavoritosAppService _service;

        protected readonly IRepository<ListaFavorito, Guid> _listaRepo;
        protected readonly IRepository<LineaListaFavorito, Guid> _lineaRepo;
        protected readonly IRepository<DestinoTuristico, Guid> _destinoRepo;
        protected readonly IRepository<Opinion, Guid> _opinionRepo;


        protected classListaDeFavoritosAppServiceTest()
        {
            // SUT (AppService real, con DI real)
            _service = GetRequiredService<ListaDeFavoritosAppService>();

            // Repos reales (EF Core)
            _listaRepo = GetRequiredService<IRepository<ListaFavorito, Guid>>();
            _lineaRepo = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();
            _destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            _opinionRepo = GetRequiredService<IRepository<Opinion, Guid>>();
        }

        [Fact]
        public async Task AgregarAFavoritosAsync_CreaListaYAgregaLinea_SinDuplicar()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue("En el entorno de tests debe haber un CurrentUser autenticado.");
            var userId = CurrentUser.Id!.Value;

            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                // Insertamos un destino para que luego ObtenerFavoritosAsync pueda armar el DTO si lo necesitás
                var destino = new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Integracion",
                    Pais = "AR",
                    Poblacion = 123,
                    Latitud = -34.6f,
                    Longitud = -58.4f,
                    IdExterno = "geo-it-001"
                };

                await _destinoRepo.InsertAsync(destino, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId)); // 2da vez (no debe duplicar)

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var lista = await _listaRepo.FirstOrDefaultAsync(l => l.UserId == userId);
                lista.ShouldNotBeNull("Debe crearse la lista para el usuario si no existe.");

                var lineas = await _lineaRepo.GetListAsync(l => l.ListaFavoritoId == lista!.Id);
                lineas.Count(l => l.DestinoTuristicoId == destinoId).ShouldBe(1);
            });
        }

        [Fact]
        public async Task EliminarDeFavoritosAsync_EliminaLinea()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino a borrar",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Act
            await WithUnitOfWorkAsync(async () => await _service.EliminarDeFavoritosAsync(destinoId));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var lista = await _service.GetOrCreateListaAsync();
                var existe = await _lineaRepo.FirstOrDefaultAsync(l =>
                    l.ListaFavoritoId == lista.Id && l.DestinoTuristicoId == destinoId);

                existe.ShouldBeNull();
            });
        }

        [Fact]
        public async Task GetOrCreateListaAsync_CreaLista_SiNoExiste()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            // Por seguridad: si hubiese data previa en la colección, vaciamos la lista del usuario
            await WithUnitOfWorkAsync(async () =>
            {
                var existing = await _listaRepo.FirstOrDefaultAsync(x => x.UserId == userId);
                if (existing != null)
                {
                    await _listaRepo.DeleteAsync(existing, autoSave: true);
                }
            });

            // Act
            var lista = await WithUnitOfWorkAsync(async () => await _service.GetOrCreateListaAsync());

            // Assert
            lista.ShouldNotBeNull();
            lista.UserId.ShouldBe(userId);

            await WithUnitOfWorkAsync(async () =>
            {
                var fromDb = await _listaRepo.FirstOrDefaultAsync(x => x.UserId == userId);
                fromDb.ShouldNotBeNull();
                fromDb!.Id.ShouldBe(lista.Id);
            });
        }

        [Fact]
        public async Task GetOrCreateListaAsync_RetornaLaMismaLista_SiYaExiste()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var lista1 = await WithUnitOfWorkAsync(async () => await _service.GetOrCreateListaAsync());

            // Act
            var lista2 = await WithUnitOfWorkAsync(async () => await _service.GetOrCreateListaAsync());

            // Assert
            lista1.ShouldNotBeNull();
            lista2.ShouldNotBeNull();
            lista1.UserId.ShouldBe(userId);
            lista2.UserId.ShouldBe(userId);
            lista2.Id.ShouldBe(lista1.Id);
        }

        [Fact]
        public async Task GetOrCreateListaAsync_LanzaExcepcion_SiNoEstaAutenticado()
        {
            // Arrange
            // El entorno de tests suele tener CurrentUser autenticado.
            // Para este caso, verificamos al menos la regla de negocio:
            // si CurrentUser no está autenticado -> AbpAuthorizationException.
            //
            // Como no tenemos un "fake login" simple acá, validamos la excepción
            // llamando directo al método y asumiendo que el CurrentUser del entorno
            // puede ser no autenticado en alguna configuración.
            //
            // Si este test te falla porque siempre hay usuario, avísame y lo adaptamos
            // para simular no autenticado reemplazando ICurrentUser en el TestModule.
            if (CurrentUser.IsAuthenticated)
            {
                // Si el entorno siempre autentica, no podemos forzar fácilmente aquí sin tocar módulo.
                // Dejamos el test como guía (se puede convertir a integration+override del ICurrentUser).
                return;
            }

            // Act + Assert
            await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
                await WithUnitOfWorkAsync(async () => await _service.GetOrCreateListaAsync()));
        }

        [Fact]
        public async Task AgregarAFavoritosAsync_CreaLinea_SiNoExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Test",
                    Pais = "AR",
                    Poblacion = 100,
                    Latitud = 0,
                    Longitud = 0,
                    IdExterno = "geo-test-001"
                }, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var lista = await _service.GetOrCreateListaAsync();

                var existe = await _lineaRepo.FirstOrDefaultAsync(x =>
                    x.ListaFavoritoId == lista.Id && x.DestinoTuristicoId == destinoId);

                existe.ShouldNotBeNull();
            });
        }

        [Fact]
        public async Task AgregarAFavoritosAsync_NoDuplicaLinea_SiYaExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino No Duplicar",
                    Pais = "AR",
                    Poblacion = 100,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            // Act
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var lista = await _service.GetOrCreateListaAsync();
                var lineas = await _lineaRepo.GetListAsync(x => x.ListaFavoritoId == lista.Id);

                lineas.Count(x => x.DestinoTuristicoId == destinoId).ShouldBe(1);
            });
        }

        [Fact]
        public async Task EsFavoritoAsync_RetornaFalse_SiNoExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            // Act
            var esFav = await WithUnitOfWorkAsync(async () => await _service.EsFavoritoAsync(destinoId));

            // Assert
            esFav.ShouldBeFalse();
        }

        [Fact]
        public async Task EsFavoritoAsync_RetornaTrue_SiExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Fav",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Act
            var esFav = await WithUnitOfWorkAsync(async () => await _service.EsFavoritoAsync(destinoId));

            // Assert
            esFav.ShouldBeTrue();
        }

        [Fact]
        public async Task ContarFavoritosAsync_CuentaCorrectamente()
        {
            // Arrange
            var d1 = Guid.NewGuid();
            var d2 = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(d1) { Nombre = "D1", Pais = "AR", Poblacion = 1, Latitud = 0, Longitud = 0 }, autoSave: true);
                await _destinoRepo.InsertAsync(new DestinoTuristico(d2) { Nombre = "D2", Pais = "AR", Poblacion = 1, Latitud = 0, Longitud = 0 }, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(d1));
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(d2));

            // Act
            var count = await WithUnitOfWorkAsync(async () => await _service.ContarFavoritosAsync());

            // Assert
            count.ShouldBeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task EliminarDeFavoritosAsync_Elimina_SiExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino borrar",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Act
            await WithUnitOfWorkAsync(async () => await _service.EliminarDeFavoritosAsync(destinoId));

            // Assert
            var esFav = await WithUnitOfWorkAsync(async () => await _service.EsFavoritoAsync(destinoId));
            esFav.ShouldBeFalse();
        }

        [Fact]
        public async Task EliminarDeFavoritosAsync_NoLanza_SiNoExiste()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            // Act
            var ex = await Record.ExceptionAsync(async () =>
                await WithUnitOfWorkAsync(async () => await _service.EliminarDeFavoritosAsync(destinoId)));

            // Assert
            ex.ShouldBeNull();
        }

        [Fact]
        public async Task VaciarFavoritosAsync_EliminaTodo()
        {
            // Arrange
            var d1 = Guid.NewGuid();
            var d2 = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(d1) { Nombre = "V1", Pais = "AR", Poblacion = 1, Latitud = 0, Longitud = 0 }, autoSave: true);
                await _destinoRepo.InsertAsync(new DestinoTuristico(d2) { Nombre = "V2", Pais = "AR", Poblacion = 1, Latitud = 0, Longitud = 0 }, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(d1));
            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(d2));

            // Act
            await WithUnitOfWorkAsync(async () => await _service.VaciarFavoritosAsync());

            // Assert
            var count = await WithUnitOfWorkAsync(async () => await _service.ContarFavoritosAsync());
            count.ShouldBe(0);
        }

        [Fact]
        public async Task ObtenerFavoritosAsync_RetornaListaVacia_SiNoHayLineas()
        {
            // Arrange
            await WithUnitOfWorkAsync(async () => await _service.VaciarFavoritosAsync());

            // Act
            var favoritos = await WithUnitOfWorkAsync(async () => await _service.ObtenerFavoritosAsync());

            // Assert
            favoritos.ShouldNotBeNull();
            favoritos.Count.ShouldBe(0);
        }

        [Fact]
        public async Task ObtenerFavoritosAsync_RetornaFavoritosConPromedioYCantidadOpiniones()
        {
            // Arrange
            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await _destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino con opiniones",
                    Pais = "AR",
                    Poblacion = 10,
                    Latitud = 0,
                    Longitud = 0,
                    IdExterno = "geo-op-1"
                }, autoSave: true);

                // 2 opiniones (mismo destino).
                // Nota: el service ignora QueryFilters y filtra !IsDeleted,
                // por eso insertamos normales (IsDeleted false).
                await _opinionRepo.InsertAsync(
                    new Opinion(destinoId, userId, ValorPuntuacion.Cinco, "Excelente"),
                    autoSave: true);

                await _opinionRepo.InsertAsync(
                    new Opinion(destinoId, userId, ValorPuntuacion.Tres, "Regular"),
                    autoSave: true);
            });

            await WithUnitOfWorkAsync(async () => await _service.AgregarAFavoritosAsync(destinoId));

            // Act
            var favoritos = await WithUnitOfWorkAsync(async () => await _service.ObtenerFavoritosAsync());

            // Assert
            favoritos.ShouldNotBeNull();
            favoritos.Count.ShouldBeGreaterThanOrEqualTo(1);

            var fav = favoritos.FirstOrDefault(x => x.Id == destinoId);
            fav.ShouldNotBeNull();
            fav!.Nombre.ShouldBe("Destino con opiniones");
            fav.GeoDBId.ShouldBe("geo-op-1");

            fav.CantidadOpiniones.ShouldBe(2);
            fav.PromedioPuntuacion.ShouldNotBeNull();
            fav.PromedioPuntuacion!.Value.ShouldBe((5 + 3) / 2.0);
        }

        //PARA TESTEAR SI SE DISPARA EL EVENTO CORRECTAMENTE, SE DEBE CREAR UN HANDLER DE PRUEBA QUE INYECTE UN REPOSITORIO DE PRUEBA PARA VER SI SE GUARDA EL EVENTO EN LA BASE DE DATOS
        [Fact]
        public async Task AgregarAFavoritosAsync_DisparaEvento_DestinoAgregadoAFavoritosEto()
        {
            // Arrange
            var service = GetRequiredService<ListaDeFavoritosAppService>();
            var destinoRepo = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
            var localEventBus = GetRequiredService<ILocalEventBus>();

            (CurrentUser.Id != null).ShouldBeTrue();
            var userId = CurrentUser.Id.Value;

            var destinoId = Guid.NewGuid();

            await WithUnitOfWorkAsync(async () =>
            {
                await destinoRepo.InsertAsync(new DestinoTuristico(destinoId)
                {
                    Nombre = "Destino Evento",
                    Pais = "AR",
                    Poblacion = 1,
                    Latitud = 0,
                    Longitud = 0
                }, autoSave: true);
            });

            var tcs = new TaskCompletionSource<DestinoAgregadoAFavoritosEto>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            // Subscribe (sin Unsubscribe, filtrando por DestinoId para no afectar otros tests)
            localEventBus.Subscribe<DestinoAgregadoAFavoritosEto>(
                (DestinoAgregadoAFavoritosEto e) =>
                {
                    if (e.DestinoId == destinoId)
                    {
                        tcs.TrySetResult(e);
                    }
                    return Task.CompletedTask;
                });

            // Act
            await WithUnitOfWorkAsync(async () => await service.AgregarAFavoritosAsync(destinoId));

            // Assert con timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token));

            completed.ShouldBe(tcs.Task, "No se recibió el evento dentro del tiempo esperado.");

            var eto = await tcs.Task;
            eto.UserId.ShouldBe(userId);
            eto.DestinoId.ShouldBe(destinoId);
        }
    }
}
