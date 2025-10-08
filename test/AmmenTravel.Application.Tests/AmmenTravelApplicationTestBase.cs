using Volo.Abp.Modularity;

namespace AmmenTravel;

public abstract class AmmenTravelApplicationTestBase<TStartupModule> : AmmenTravelTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
