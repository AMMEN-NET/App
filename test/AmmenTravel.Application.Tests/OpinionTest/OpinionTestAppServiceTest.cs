using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace AmmenTravel.OpinionTest
{
    public abstract class OpinionTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly ICreateUpdateOpinion _createUpdateOpinion;
        private readonly IOpinionAppService _opinionService;

        protected OpinionTestAppServiceTest()
        {
            _createUpdateOpinion = GetRequiredService<ICreateUpdateOpinion>();
            _opinionService = GetRequiredService<IOpinionAppService>();
        }
        [Fact]

        public async Task CrearOpinionAsync_ShouldReturnOpinionDto()
        {
            var input = new Opiniones.OpinionesDTO.createUpdateOpinionDto
            {
                DestinoTuristicoId = Guid.NewGuid(),
                Puntuacion = ValorPuntuacion.Cinco,
                Comentario = "Excelente destino turístico!"
            };

            // Act
            var result = await _opinionService.CrearOpinionAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.DestinoTuristicoId.ShouldBe(input.DestinoTuristicoId);
            result.Puntuacion.ShouldBe(input.Puntuacion);
            ((int)result.Puntuacion).ShouldBeGreaterThanOrEqualTo((int)ValorPuntuacion.Uno);
            ((int)result.Puntuacion).ShouldBeLessThanOrEqualTo((int)ValorPuntuacion.Cinco);
            result.Comentario.ShouldBe(input.Comentario);
        }

        [Fact]

        public async Task CrearOpinionAsync_NoDebePermitirDuplicados()
        {
            // Arrange
            var destinoId = Guid.NewGuid();

            var input = new createUpdateOpinionDto
            {
                DestinoTuristicoId = destinoId,
                Puntuacion = ValorPuntuacion.Cuatro,
                Comentario = "Muy lindo lugar"
            };

            // Crear primera opinión
            var primeraOpinion = await _opinionService.CrearOpinionAsync(input);

            // Act + Assert: intentar duplicar
            var ex = await Assert.ThrowsAsync<UserFriendlyException>(() => _opinionService.CrearOpinionAsync(input));

            ex.Message.ShouldBe("Ya has calificado este destino.");
        }

    }






}
