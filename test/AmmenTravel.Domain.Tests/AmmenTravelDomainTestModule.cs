using Volo.Abp.Modularity;

namespace AmmenTravel;

[DependsOn(
    typeof(AmmenTravelDomainModule),
    typeof(AmmenTravelTestBaseModule)
)]
public class AmmenTravelDomainTestModule : AbpModule
{

}
