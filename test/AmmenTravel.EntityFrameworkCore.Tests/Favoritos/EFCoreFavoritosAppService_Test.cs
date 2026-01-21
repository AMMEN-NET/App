using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.Favorito_Test;
using Xunit;

namespace AmmenTravel.Favoritos
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreFavoritosAppService_Test : ListaDeFavoritosAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}