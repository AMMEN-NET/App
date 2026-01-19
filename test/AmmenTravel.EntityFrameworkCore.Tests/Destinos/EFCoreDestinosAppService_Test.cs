using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AmmenTravel.DestinoTest;
using AmmenTravel.EntityFrameworkCore;

namespace AmmenTravel.Destinos
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreDestinosAppService_Test : classCuentaTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
