using AmmenTravel.Cuentas;
using AmmenTravel.Opiniones;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Xunit;
using static AmmenTravel.Permissions.AmmenTravelPermissions;


namespace AmmenTravel.CuentaTest
{
    public abstract class classCuentaTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly ICurrentUser _usuarioActual;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<Opinion, Guid> _opinionRepository;
        private readonly CuentaAppService _sut; // System Under Test

        protected classCuentaTestAppServiceTest()
        {
            // Creamos los mocks
            _usuarioActual = Substitute.For<ICurrentUser>();
            _userManager = Substitute.For<IdentityUserManager>(
                Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null
            );
            _opinionRepository = Substitute.For<IRepository<Opinion, Guid>>();

            // Inyectamos los mocks en el servicio
            _sut = new CuentaAppService(_usuarioActual, _userManager, _opinionRepository);
        }

        [Fact]
        public async Task EliminarMiCuentaAsync_EliminarOpinionesYUsuario()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _usuarioActual.Id.Returns(usuarioId);

            var destinoId = Guid.NewGuid();
            var puntuacion = ValorPuntuacion.Cinco;
            var comentario = "Muy buena experiencia";
            var opiniones = new List<Opinion>
           {
               new Opinion(destinoId, usuarioId, puntuacion, comentario),
               new Opinion(destinoId, usuarioId, puntuacion, comentario) };

            _opinionRepository.GetListAsync(x => x.UserId == usuarioId)
            .Returns(opiniones);

            var usuario = new IdentityUser(usuarioId, "testuser", "test@mail.com");
            _userManager.GetByIdAsync(usuarioId).Returns(usuario);

            // Act
            await _sut.EliminarMiCuentaAsync();

            // Assert
            await _opinionRepository.Received(opiniones.Count).DeleteAsync(Arg.Any<Opinion>());
            await _userManager.Received(1).DeleteAsync(usuario);
        }

        [Fact]
        public async Task EliminarMiCuentaAsync_NoHaceNada_SiUsuarioEsNull()
        {
            // Arrange
            _usuarioActual.Id.Returns((Guid?)null);

            // Act
            await _sut.EliminarMiCuentaAsync();

            // Assert
            await _opinionRepository.DidNotReceive().DeleteAsync(Arg.Any<Opinion>());
            await _userManager.DidNotReceive().DeleteAsync(Arg.Any<IdentityUser>());
        }
    }
}

