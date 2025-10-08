using Volo.Abp.Modularity;

namespace AmmenTravel;

[DependsOn(
    typeof(AmmenTravelApplicationModule),
    typeof(AmmenTravelDomainTestModule)
)]
public class AmmenTravelApplicationTestModule : AbpModule
{

}
