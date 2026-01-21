using AmmenTravel.EntityFrameworkCore;
using Xunit;
using AmmenTravel.OpinionTest;


namespace AmmenTravel.Opiniones
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreOpinionesAppService_Test : OpinionTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}