using AmmenTravel.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AmmenTravel.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AmmenTravelController : AbpControllerBase
{
    protected AmmenTravelController()
    {
        LocalizationResource = typeof(AmmenTravelResource);
    }
}
