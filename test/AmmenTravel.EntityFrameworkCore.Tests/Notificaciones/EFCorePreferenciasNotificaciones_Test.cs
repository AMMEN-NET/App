using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.NotificacionTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Notificaciones
{

    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCorePreferenciasNotificaciones_Test : PreferenciasNotificacionTestAppService<AmmenTravelEntityFrameworkCoreTestModule>
    {

    }
}
