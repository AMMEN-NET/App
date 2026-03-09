using AmmenTravel.Destinos;
using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.InterfaceDestinoAppService;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xunit;

namespace AmmenTravel.Favoritos
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreFavoritosAppService_Test : classListaDeFavoritosAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
        
    }
}