using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using AmmenTravel.Application.ExternalServices;
using AmmenTravel.ExternalService;

namespace AmmenTravel;

[DependsOn(
    typeof(AmmenTravelDomainModule),
    typeof(AmmenTravelApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]


public class AmmenTravelApplicationModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AmmenTravelApplicationModule>();
        });

        // Registro de GeoDbCitySearchService como implementación de ICitySearchService
        context.Services.AddTransient<IBuscarCiudadService, GeoDdBuscarCiudadService>();
    }

}