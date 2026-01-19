using AmmenTravel;
using AmmenTravel.Destinos;
using AmmenTravel.ExternalService;
using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Users;
using Xunit;

namespace AmmenTravel.OpinionTest
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public abstract class OpinionTestAppServiceTest<TStartupModule>
        : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IOpinionAppService _opinionService;
        private readonly IRepository<DestinoTuristico, Guid> _destinoRepository;

        protected OpinionTestAppServiceTest()
        {
            _opinionService = GetRequiredService<IOpinionAppService>();
            _destinoRepository = GetRequiredService<IRepository<DestinoTuristico, Guid>>();
        }

        [Fact]
        public async Task CrearOpinionAsync_DebeRetornarOpinionDto()
        {
            var destinoId = Guid.NewGuid();
            var destino = new DestinoTuristico(destinoId)
            {
                Nombre = "Destino Test",
                Pais = "Argentina",
                Poblacion = 100000,
                Latitud = -34.6f,
                Longitud = -58.4f,
                IdExterno = "test-geo-001"
            };

            await _destinoRepository.InsertAsync(destino);

           

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
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

            var destino = new DestinoTuristico(destinoId)
            {
                Nombre = "Destino Test",
                Pais = "Argentina",
                Poblacion = 100000,
                Latitud = -34.6f,
                Longitud = -58.4f,
                IdExterno = "test-geo-002"
            };

            await _destinoRepository.InsertAsync(destino);


            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Cuatro,
                Comentario = "Muy lindo lugar"
            };

            var primeraOpinion = await _opinionService.CrearOpinionAsync(input);

            var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => _opinionService.CrearOpinionAsync(input));
            ex.Message.ShouldBe("Ya calificaste Destino Test. Podes actualizar tu opinión en la sección 'Mis calificaciones' si lo deseas.");
        }

        [Fact]
        public async Task Debe_RespetarFiltroPorUsuario_Y_RequerirAutenticacion()
        {
            // Requisito 1: Requerir Autenticación (se verifica al inicio)
            CurrentUser.IsAuthenticated.ShouldBeTrue();

            var destinoId = Guid.NewGuid();

            var destino = new DestinoTuristico(destinoId)
            {
                Nombre = "Destino Filtro Usuario",
                Pais = "Argentina",
                Poblacion = 50000,
                Latitud = -34.5f,
                Longitud = -58.3f,
                IdExterno = "test-geo-003"
            };

            await _destinoRepository.InsertAsync(destino);

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Tres,
                Comentario = "Correcto."
            };

            var opinion = await _opinionService.CrearOpinionAsync(input);

            // Requisito 2: Respetar Filtro por Usuario (El usuario solo ve su propia opinión)
            var currentUserId = CurrentUser.Id.Value;
            var opinionesUsuario = await _opinionService.ObtenerPorUsuarioAsync(currentUserId);
            opinionesUsuario.ShouldContain(o => o.Id == opinion.Id);

            // 🔸 Simular un contexto sin autenticación
            var currentUserMock = GetRequiredService<ICurrentUser>();
            currentUserMock.IsAuthenticated.Returns(false);
            currentUserMock.Id.Returns((Guid?)null);

            // Verificar que al intentar la operación sin autenticación, se lance la excepción de autorización
            await Should.ThrowAsync<AbpAuthorizationException>(
                async () => await _opinionService.ObtenerPorUsuarioAsync(Guid.NewGuid())
            );
        }


        //Asegurar que el endpoint de crear una opinion falla con 401 si no se provee token

        [Fact]
        public async Task CrearOpinionAsync_DebeFallarCon401SiNoSeProveeToken()
        {
            // Simular un contexto sin autenticación
            CurrentUser.IsAuthenticated.Returns(false);
            CurrentUser.Id.Returns((Guid?)null);
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = Guid.NewGuid(),
                Puntuacion = ValorPuntuacion.Dos,
                Comentario = "No me gustó mucho."
            };

            // Verificar que al intentar crear una opinión sin autenticación, se lance la excepción de autorización de ABP.
            await Should.ThrowAsync<AbpAuthorizationException>(
                async () => await _opinionService.CrearOpinionAsync(input)
            );



        }

    }
}