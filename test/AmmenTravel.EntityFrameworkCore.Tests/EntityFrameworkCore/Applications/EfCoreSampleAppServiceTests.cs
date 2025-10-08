using AmmenTravel.Samples;
using Xunit;

namespace AmmenTravel.EntityFrameworkCore.Applications;

[Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AmmenTravelEntityFrameworkCoreTestModule>
{

}
