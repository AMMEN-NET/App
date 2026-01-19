using AmmenTravel.DestinoTest;
using AmmenTravel.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Cuentas
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreCuentasAppService_Test : classCuentaTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
