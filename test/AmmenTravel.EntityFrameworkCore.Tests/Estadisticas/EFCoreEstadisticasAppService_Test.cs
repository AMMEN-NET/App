using AmmenTravel.EntityFrameworkCore;
using AmmenTravel.EstadisticaTest;
using AmmenTravel.ExperienciaTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AmmenTravel.Estadisticas
{
    [Collection(AmmenTravelTestConsts.CollectionDefinitionName)]
    public class EFCoreEstadisticasAppService_Test : classEstadisticaAppServiceTest<AmmenTravelEntityFrameworkCoreTestModule>
    {
    }
}
