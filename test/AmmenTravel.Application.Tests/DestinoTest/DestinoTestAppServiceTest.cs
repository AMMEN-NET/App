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


namespace AmmenTravel.DestinoTest
{
    public abstract class classDestinoTestAppServiceTest<TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IDestinoAppService _service;

        protected classDestinoTestAppServiceTest()
        {
            _service = GetRequiredService<IDestinoAppService>();
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
    }


}