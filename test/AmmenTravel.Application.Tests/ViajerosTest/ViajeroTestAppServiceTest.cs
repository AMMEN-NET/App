using AmmenTravel.Destinos;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using AmmenTravel.Viajeros;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.ViajerosTest
{
    public abstract class ViajeroTestAppServiceTest<TStartupModule>
   : AmmenTravelApplicationTestBase<TStartupModule>
   where TStartupModule : IAbpModule
    {
        private readonly IViajerosAppService _viajerosAppService;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly IRepository<ListaFavorito, Guid> _listaFavoritoRepository;
        private readonly IRepository<LineaListaFavorito, Guid> _lineaFavoritoRepository;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;

        public ViajeroTestAppServiceTest()
        {
            _viajerosAppService = GetRequiredService<IViajerosAppService>();
            _userRepository = GetRequiredService<IIdentityUserRepository>();
            _opinionRepository = GetRequiredService<IRepository<Opinion, Guid>>();
            _listaFavoritoRepository = GetRequiredService<IRepository<ListaFavorito, Guid>>();
            _lineaFavoritoRepository = GetRequiredService<IRepository<LineaListaFavorito, Guid>>();
            _destinoRepository = GetRequiredService<IRepository<DestinoTuristico, Guid>>();


        }

        [Fact]
        public async Task Should_Get_List_Of_Viajeros()
        {
            // Arrange: asegurar que exista al menos un usuario con UserName conocido
            var testUserName = "test.user";
            var existing = (await _userRepository.GetListAsync(filter: testUserName)).FirstOrDefault();
            if (existing == null)
            {
                var user = new IdentityUser(Guid.NewGuid(), testUserName, "test@local");
                await _userRepository.InsertAsync(user);
            }

            // Act
            var list = await _viajerosAppService.GetListAsync(null);

            // Assert
            list.ShouldNotBeNull();
            list.Count.ShouldBeGreaterThan(0);
            list.Any(v => v.UserName == testUserName).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Get_PerfilPublico_WithCounts()
        {
            // Arrange: crear usuario, opiniones, lista y lineas
            var userId = Guid.NewGuid();
            var destinoId = Guid.NewGuid();

            var user = new IdentityUser(userId, "perfil.user", "perfil@local");
            await _userRepository.InsertAsync(user);

            var destino = new DestinoTuristico(destinoId)
            {
                Nombre = "Destino Test",
                Pais = "Argentina",
                Poblacion = 100000,
                Latitud = -32.9f,
                Longitud = -68.8f,
                IdExterno = "geo-test-123"
            };
            await _destinoRepository.InsertAsync(destino);


            // Crear 2 opiniones del usuario y 1 de otro
            await _opinionRepository.InsertAsync(new Opinion(destinoId, userId, ValorPuntuacion.Cinco, "Muy bueno"));
            await _opinionRepository.InsertAsync(new Opinion(destinoId, userId, ValorPuntuacion.Cuatro, "Recomendado"));
            await _opinionRepository.InsertAsync(new Opinion(destinoId, Guid.NewGuid(), ValorPuntuacion.Tres, "Otro usuario"));

            // Crear lista de favoritos del usuario y 2 lineas
            var lista = new ListaFavorito
            { UserId = userId };
            await _listaFavoritoRepository.InsertAsync(lista);

            // Crear 2 líneas de favoritos usando el Id generado automáticamente }
            await _lineaFavoritoRepository.InsertAsync(new LineaListaFavorito
            {
                ListaFavoritoId = lista.Id,
                DestinoTuristicoId = destinoId
            });
            await _lineaFavoritoRepository.InsertAsync(new LineaListaFavorito
            { ListaFavoritoId = lista.Id, DestinoTuristicoId = destinoId });
            // Act
            var perfil = await _viajerosAppService.GetPerfilPublicoAsync(userId);

            // Assert
            perfil.ShouldNotBeNull();
            perfil.Id.ShouldBe(userId);
            perfil.UserName.ShouldBe("perfil.user");
            perfil.CantidadOpiniones.ShouldBe(2);
            perfil.CantidadFavoritos.ShouldBe(2);
        }

         [Fact]
        public async Task Should_Get_PerfilPublico_When_No_List_Returns_Zero_Favorites()
        {
            //Arrange: usuario sin lista de favoritos
            var userId = Guid.NewGuid();
            var user = new IdentityUser(userId, "nolista.user", "nolista@local");
            await _userRepository.InsertAsync(user);

            // Act
            var perfil = await _viajerosAppService.GetPerfilPublicoAsync(userId);

            // Assert
            perfil.ShouldNotBeNull();
            perfil.Id.ShouldBe(userId);
            perfil.CantidadFavoritos.ShouldBe(0);
            perfil.CantidadOpiniones.ShouldBe(0);
        }
    }
}
