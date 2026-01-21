﻿using AmmenTravel.DestinoTest;
using AmmenTravel.EntityFrameworkCore;
using Xunit;

namespace AmmenTravel.Cuentas
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreCuentasAppService_Test : classCuentaTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}