using Volo.Abp.Modularity;
using Volo.Abp.Users;


namespace AmmenTravel;

public abstract class AmmenTravelApplicationTestBase<TStartupModule>
    : AmmenTravelTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    //  Expone al usuario actual para los tests
    protected ICurrentUser CurrentUser => GetRequiredService<ICurrentUser>();
}