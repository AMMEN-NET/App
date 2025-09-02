using AmmenTravel.Samples;
using Xunit;

namespace AmmenTravel.EntityFrameworkCore.Domains;

[Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AmmenTravelEntityFrameworkCoreTestModule>
{

}
