using AmmenTravel.ExternalService;
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
        private readonly IBuscarCiudadService _buscarCiudadService;

        protected OpinionTestAppServiceTest()
        {
            _opinionService = GetRequiredService<IOpinionAppService>();
            // Nota: En un entorno de prueba de ABP, ICurrentUser se mockea
            // o se proporciona con un usuario de prueba autenticado por defecto.
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

            // Intenta crear la segunda opinión y espera la excepción
            var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => _opinionService.CrearOpinionAsync(input));
            ex.Message.ShouldBe("Ya has calificado este destino.");
        }

        [Fact]
        public async Task Debe_RespetarFiltroPorUsuario_Y_RequerirAutenticacion()
        {
            // Requisito 1: Requerir Autenticación (se verifica al inicio)
            // Se asume que el usuario de prueba está autenticado al iniciar el test.
            _currentUser.IsAuthenticated.ShouldBeTrue();

            var destinoId = Guid.NewGuid();

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Tres,
                Comentario = "Correcto."
            };

            var opinion = await _opinionService.CrearOpinionAsync(input);

            // Requisito 2: Respetar Filtro por Usuario (El usuario solo ve su propia opinión)
            var opinionesUsuario = await _opinionService.ObtenerPorUsuarioAsync(_currentUser.Id.Value);
            opinionesUsuario.ShouldContain(o => o.Id == opinion.Id);

            // 🔸 Simular un contexto sin autenticación (Usando NSubstitute, asumiendo ICurrentUser es un mock)
            // Nota: Si ICurrentUser no es un Mock, esta línea debe ser adaptada al framework de testing de ABP.
            // Para el contexto de pruebas ABP, a menudo se usa un 'using (AbpSession.Use(null))' o se ajusta el mock del usuario
            // para el scope de la llamada.
            _currentUser.IsAuthenticated.Returns(false);
            _currentUser.Id.Returns((Guid?)null);

            // Verificar que al intentar la operación sin autenticación, se lance la excepción de autorización de ABP.
            await Should.ThrowAsync<AbpAuthorizationException>(
                async () => await _opinionService.ObtenerPorUsuarioAsync(Guid.NewGuid())
            );
        }
    }
}