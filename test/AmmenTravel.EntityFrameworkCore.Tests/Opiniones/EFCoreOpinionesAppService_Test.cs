using AmmenTravel.DestinoTest;
using AmmenTravel.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using AmmenTravel.OpinionTest;


namespace AmmenTravel.Opiniones
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreOpinionesAppService_Test: OpinionTestAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
