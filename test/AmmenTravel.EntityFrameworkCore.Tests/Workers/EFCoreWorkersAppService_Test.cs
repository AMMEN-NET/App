using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.NotificacionTest;
using AmmenTravel.WorkersTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Workers
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreWorkersAppService_Test : WorkersTestAppService<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
