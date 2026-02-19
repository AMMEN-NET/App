using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.ExperienciaTest;
using Xunit;

namespace AmmenTravel.Experiencias
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreExperienciasAppService_Test : classExperienciaTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}