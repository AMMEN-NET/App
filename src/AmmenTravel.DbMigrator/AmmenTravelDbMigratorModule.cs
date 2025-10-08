using AmmenTravel.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AmmenTravel.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AmmenTravelEntityFrameworkCoreModule),
    typeof(AmmenTravelApplicationContractsModule)
)]
public class AmmenTravelDbMigratorModule : AbpModule
{
}
