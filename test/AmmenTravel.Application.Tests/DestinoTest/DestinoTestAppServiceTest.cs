using AmmenTravel;
using AmmenTravel.DestinosDTO;
using AmmenTravel.Destinos;
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
using NSubstitute;
using AmmenTravel.ExternalService;
using Volo.Abp.Domain.Repositories;


namespace AmmenTravel.DestinoTest
{
    public abstract class classDestinoTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IBuscarCiudadService _buscarCiudadService;
        private readonly IDestinoAppService _service;
        private readonly IDbContextProvider<AmmenTravelDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        protected classDestinoTestAppServiceTest()
        {
            _service = GetRequiredService<IDestinoAppService>();
            _dbContextProvider = GetRequiredService<IDbContextProvider<AmmenTravelDbContext>>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
            _buscarCiudadService = Substitute.For<IBuscarCiudadService>();
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

        [Fact]

        public async Task buscarCiudadesAsync_resultado()
        {
            var request = new CiudadBuscadaDTO { Nombre = "Test" };
            var expectedResponse = new CiudadResultadoDTO
            {
                Ciudades = new List<CiudadDTO>
                {
                    new CiudadDTO { Nombre = "TestCiudad1", Pais = "TestPais1" },
                    new CiudadDTO { Nombre = "TestCiudad2", Pais = "TestPais2" }
                }
            };
            var repomock = Substitute.For<IRepository<DestinoTuristico,Guid>>();
            var ciudadBuscadaMock = Substitute.For<IBuscarCiudadService>();
            ciudadBuscadaMock.BuscarCiudadesAsync(request).Returns(expectedResponse);
            var service = new DestinoAppService(repomock, ciudadBuscadaMock);
            var result = await service.BuscarCiudadesAsync(request);
            result.ShouldNotBeNull();
            result.Ciudades.Count.ShouldBe(2);
            result.Ciudades[0].Nombre.ShouldBe("TestCiudad1");
        }

        [Fact]

        public async Task buscarCiudadesAsync_RetornoVacio()         {
            var request = new CiudadBuscadaDTO { Nombre = "" };
            var expectedResponse = new CiudadResultadoDTO
            {
                Ciudades = new List<CiudadDTO>()
            };
            var repomock = Substitute.For<IRepository<DestinoTuristico, Guid>>();
            var ciudadBuscadaMock = Substitute.For<IBuscarCiudadService>();
            ciudadBuscadaMock.BuscarCiudadesAsync(request).Returns(expectedResponse);
            var service = new DestinoAppService(repomock, ciudadBuscadaMock);
            var result = await service.BuscarCiudadesAsync(request);
            result.ShouldNotBeNull();
            result.Ciudades.Count.ShouldBe(0);
        }

        [Fact]

        public async Task buscarCiudadesAsync_InputInvalido_RetornoVacio()
        {
            var request = new CiudadBuscadaDTO { Nombre = null };
            var expectedResponse = new CiudadResultadoDTO
            {
                Ciudades = new List<CiudadDTO>()
            };
            var repomock = Substitute.For<IRepository<DestinoTuristico, Guid>>();
            var ciudadBuscadaMock = Substitute.For<IBuscarCiudadService>();
            ciudadBuscadaMock.BuscarCiudadesAsync(request).Returns(expectedResponse);
            var service = new DestinoAppService(repomock, ciudadBuscadaMock);
            var result = await service.BuscarCiudadesAsync(request);
            result.ShouldNotBeNull();
            result.Ciudades.Count.ShouldBe(0);
        }

        [Fact]

        public async Task buscarCiudadesAsync_APIError_LanzaExcepcion()       {
            var request = new CiudadBuscadaDTO { Nombre = "ErrorCity" };
            var repomock = Substitute.For<IRepository<DestinoTuristico, Guid>>();
            var ciudadBuscadaMock = Substitute.For<IBuscarCiudadService>();
            ciudadBuscadaMock
                .When(x => x.BuscarCiudadesAsync(request))
                .Do(x => { throw new Exception("Error al buscar ciudades"); });
            var service = new DestinoAppService(repomock, ciudadBuscadaMock);
            await Should.ThrowAsync<Exception>(async () =>
            {
                await service.BuscarCiudadesAsync(request);
            });
        }



    }

}