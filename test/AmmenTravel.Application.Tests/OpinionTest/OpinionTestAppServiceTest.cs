using AmmenTravel.Opiniones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace AmmenTravel.OpinionTest
{
    public abstract class OpinionTestAppServiceTest <TStartupModule> : AmmenTravelApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        IOpinionAppService _OpinionService;
    }
}
