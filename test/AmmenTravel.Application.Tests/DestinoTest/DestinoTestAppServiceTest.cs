using AmmenTravel;
using AmmenTravel.DestinosDTO;
using AmmenTravel.InterfaceDestinoAppService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using Shouldly;
using Volo.Abp.Uow;
using AmmenTravel.EntityFrameworkCore;
using JetBrains.Annotations;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Validation;


namespace AmmenTravel.DestinoTest
{
    public abstract class classDestinoTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IDestinoAppService _service;
        private readonly IDbContextProvider<AmmenTravelDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;


        protected classDestinoTestAppServiceTest()
        {
            _service = GetRequiredService<IDestinoAppService>();
            _dbContextProvider = GetRequiredService<IDbContextProvider<AmmenTravelDbContext>>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreateDestinoDTO()
        {
            var input = new CreateUpdateDestinoDTO
            {
                Nombre = "Paris",
                Pais = "Francia",
                Poblacion = 2048000,
                FotoURL = "https://example.com/paris.jpg"
            };
                
            //Act
            var result = await _service.CreateAsync(input);

            //Assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Nombre.ShouldBe(input.Nombre);
            result.Pais.ShouldBe(input.Pais);
            result.Poblacion.ShouldBe(input.Poblacion);
            result.FotoURL.ShouldBe(input.FotoURL);
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistDestinoInDatabase()
        {

            using (var uow = _unitOfWorkManager.Begin())
            {
                //Arrange
                var input = new CreateUpdateDestinoDTO
                {
                    Nombre = "Tokio",
                    Pais = "Japon",
                    Poblacion = 13960000,
                    FotoURL = "https://ejemplo.com/tokyo.jpg"
                };

                //Act
                var result = await _service.CreateAsync(input);


                //Assert
                var dbContext = await _dbContextProvider.GetDbContextAsync();
                var savedDestino = await dbContext.Destinos.FindAsync(result.Id);

                savedDestino.ShouldNotBeNull();
                savedDestino.Nombre.ShouldBe(input.Nombre);
                savedDestino.Pais.ShouldBe(input.Pais);
                savedDestino.Poblacion.ShouldBe(input.Poblacion);
                savedDestino.FotoURL.ShouldBe(input.FotoURL);
            }

        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNombreIsMissing()
        {
            //Arrange
            var input = new CreateUpdateDestinoDTO
            {
                Nombre = null, // Falta Nombre
                Pais = "Belgica",
                Poblacion = 195500,
                FotoURL = "https://example.com/rome.jpg"
            };

            //Act & Assert
            await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _service.CreateAsync(input);
            });
        }

    }


}