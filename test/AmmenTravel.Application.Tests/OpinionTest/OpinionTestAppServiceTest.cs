using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Users;
using Xunit;


namespace AmmenTravel.OpinionTest
{
    public abstract class OpinionTestAppServiceTest<TStartupModule>
        : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IOpinionAppService _opinionService;
        private readonly ICurrentUser _currentUser;

        protected OpinionTestAppServiceTest()
        {
            _opinionService = GetRequiredService<IOpinionAppService>();
            _currentUser = GetRequiredService<ICurrentUser>();
        }

        [Fact]
        public async Task CrearOpinionAsync_ShouldReturnOpinionDto()
        {
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = Guid.NewGuid(),
                Puntuacion = ValorPuntuacion.Cinco,
                Comentario = "Excelente destino turístico!"
            };

            var result = await _opinionService.CrearOpinionAsync(input);

            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.DestinoTuristicoId.ShouldBe(input.DestinoTuristicoId);
            result.Puntuacion.ShouldBe(input.Puntuacion);
            ((int)result.Puntuacion).ShouldBeInRange((int)ValorPuntuacion.Uno, (int)ValorPuntuacion.Cinco);
            result.Comentario.ShouldBe(input.Comentario);
        }

        [Fact]
        public async Task CrearOpinionAsync_NoDebePermitirDuplicados()
        {
            var destinoId = Guid.NewGuid();

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Cuatro,
                Comentario = "Muy lindo lugar"
            };

            var primeraOpinion = await _opinionService.CrearOpinionAsync(input);

            var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => _opinionService.CrearOpinionAsync(input));
            ex.Message.ShouldBe("Ya has calificado este destino.");
        }

        [Fact]
        public async Task Debe_RespetarFiltroPorUsuario_Y_RequerirAutenticacion()
        {
            // El usuario actual está autenticado
            _currentUser.IsAuthenticated.ShouldBeTrue();

            var destinoId = Guid.NewGuid();

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Tres,
                Comentario = "Correcto."
            };

            var opinion = await _opinionService.CrearOpinionAsync(input);

            var opinionesUsuario = await _opinionService.ObtenerPorUsuarioAsync(_currentUser.Id.Value);
            opinionesUsuario.ShouldContain(o => o.Id == opinion.Id);

            // 🔸 Simular un contexto sin autenticación
            _currentUser.IsAuthenticated.Returns(false);
            _currentUser.Id.Returns((Guid?)null);

            await Should.ThrowAsync<AbpAuthorizationException>(
                async () => await _opinionService.ObtenerPorUsuarioAsync(Guid.NewGuid())
            );
        }

    }


}
