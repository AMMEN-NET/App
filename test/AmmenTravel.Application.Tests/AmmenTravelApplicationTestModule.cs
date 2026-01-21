using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp.Modularity;
using Volo.Abp;
using AmmenTravel.ExternalService;
using AmmenTravel.Opiniones;

namespace AmmenTravel;

[DependsOn(
    typeof(AmmenTravelApplicationModule),
    typeof(AmmenTravelDomainTestModule)
)]
public class AmmenTravelApplicationTestModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Siempre usar un mock para ICitySearchService en los tests
        var citySearchServiceMock = Substitute.For<IBuscarCiudadService>();
        context.Services.AddSingleton(citySearchServiceMock);
    }

}