using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.NotificacionTest;
using Xunit;

namespace AmmenTravel.Notificaciones
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreNotificacionesAppService_Test : classNotificacionTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}