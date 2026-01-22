using AmmenTravel.ExternalService;
using AmmenTravel.Opiniones;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace AmmenTravel;

[DependsOn(
    typeof(AmmenTravelApplicationModule),
    typeof(AmmenTravelDomainTestModule),
    typeof(AbpPermissionManagementDomainModule), // <--- AGREGAR ESTA
    typeof(AbpSettingManagementDomainModule)
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