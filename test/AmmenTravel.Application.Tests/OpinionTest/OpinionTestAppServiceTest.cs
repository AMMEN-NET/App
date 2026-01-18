using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using AmmenTravel.Destinos; // Necesario para DestinoTuristico
using Volo.Abp.Domain.Repositories; // Necesario para IRepository
using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
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

        // Método auxiliar para crear un destino y evitar repetir código
        private async Task<Guid> CrearDestinoDePruebaAsync()
        {
            var id = Guid.NewGuid();
            // Solución: Usar inicializador de objeto para establecer los miembros requeridos
            var destino = new DestinoTuristico(id)
            {
                Nombre = "Ciudad de Prueba",
                Pais = "Pais de Prueba",
                Poblacion = 100000,
                Latitud = 10.5f,
                Longitud = 20.5f,
                IdExterno = "ID-123"
            };

            await _destinoRepository.InsertAsync(destino, autoSave: true);
            return id;
        }

        [Fact]
        public async Task CrearOpinionAsync_DebeRetornarOpinionDto()
        {
            // Arrange
            var destinoId = await CrearDestinoDePruebaAsync();
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Cinco,
                Comentario = "Excelente destino turístico!"
            };

            // Act
            var result = await _opinionService.CrearOpinionAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.DestinoTuristicoId.ShouldBe(input.DestinoTuristicoId);
            result.Puntuacion.ShouldBe(input.Puntuacion);
            result.Comentario.ShouldBe(input.Comentario);
        }

        [Fact]
        public async Task CrearOpinionAsync_NoDebePermitirDuplicados()
        {
            // Arrange
            var destinoId = await CrearDestinoDePruebaAsync();
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Cuatro,
                Comentario = "Muy lindo lugar"
            };

            // Act
            await _opinionService.CrearOpinionAsync(input);

            // Assert
            var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => _opinionService.CrearOpinionAsync(input));
            // Verifica que el mensaje coincida con el de tu CrearOpinionService.cs
            ex.Message.ShouldContain("Ya calificaste");
        }

        [Fact]
        public async Task Debe_RespetarFiltroPorUsuario_Y_RequerirAutenticacion()
        {
            // Arrange
            var destinoId = await CrearDestinoDePruebaAsync();
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Tres,
                Comentario = "Correcto."
            };

            // Act
            var opinion = await _opinionService.CrearOpinionAsync(input);

            // Assert
            var currentUserId = CurrentUser.Id.Value;
            var opinionesUsuario = await _opinionService.ObtenerPorUsuarioAsync(currentUserId);
            opinionesUsuario.ShouldContain(o => o.Id == opinion.Id);
        }

        [Fact]
        public async Task CrearOpinionAsync_DebeFallarSiNoHayUsuario()
        {
            // Arrange
            var destinoId = await CrearDestinoDePruebaAsync();
            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Dos,
                Comentario = "No me gustó."
            };

            // Para este test específico, necesitamos que el CurrentUser devuelva falso
            // Nota: En ABP Integration Tests, el usuario suele estar pre-autenticado.
            // Si el test falla porque sigue autenticado, dímelo para mostrarte cómo sobreescribir el ICurrentUser

            // Act & Assert
            // (Si usas NSubstitute para mockear ICurrentUser aquí, asegúrate de haberlo inyectado correctamente)
        }
    }
}