using AmmenTravel.Localization;
using Volo.Abp.Application.Services;

namespace AmmenTravel;

/* Inherit your application services from this class.
 */
public abstract class AmmenTravelAppService : ApplicationService
{
    protected AmmenTravelAppService()
    {
        LocalizationResource = typeof(AmmenTravelResource);
    }
}
