using Xunit;

namespace AmmenTravel.EntityFrameworkCore;

[CollectionDefinition(AmmenTravelTestConsts.CollectionDefinitionName)]
public class AmmenTravelEntityFrameworkCoreCollection : ICollectionFixture<AmmenTravelEntityFrameworkCoreFixture>
{

}
