using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.ViajerosTest;
using Xunit;

namespace AmmenTravel.Viajeros
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreViajerosAppService_Test : ViajeroTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}