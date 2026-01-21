using Xunit;
using AmmenTravel.DestinoTest;
using AmmenTravel.EntityFrameworkCore;

namespace AmmenTravel.Destinos
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreDestinosAppService_Test : classCuentaTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}