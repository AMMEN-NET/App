using AmmenTravel.Destinos;
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
    public class EFCoreFavoritosAppService_Test : ListaDeFavoritosAppServiceTest
    {
        public EFCoreFavoritosAppService_Test(IRepository<ListaFavorito, Guid> listaRepository, IRepository<LineaListaFavorito, Guid> lineaRepository, ICurrentUser currentUser, IDestinoAppService destinoAppService, IRepository<DestinoTuristico, Guid> destinoRepository, IRepository<Opinion, Guid> opinionRepository) : base(listaRepository, lineaRepository, currentUser, destinoAppService, destinoRepository, opinionRepository)
        {
        }
    }
}