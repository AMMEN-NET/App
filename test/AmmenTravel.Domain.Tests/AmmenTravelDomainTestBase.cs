using Volo.Abp.Modularity;

namespace AmmenTravel;

/* Inherit from this class for your domain layer tests. */
public abstract class AmmenTravelDomainTestBase<TStartupModule> : AmmenTravelTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
