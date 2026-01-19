using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.Favorito_Test;
using AmmenTravel.OpinionTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Favoritos
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreFavoritosAppService_Test : ListaDeFavoritosAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
