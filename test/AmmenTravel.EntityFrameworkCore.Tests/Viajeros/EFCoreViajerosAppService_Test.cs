using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.ViajerosTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Viajeros
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreViajerosAppService_Test : ViajeroTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
