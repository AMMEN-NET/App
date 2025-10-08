using Microsoft.Extensions.Localization;
using AmmenTravel.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AmmenTravel;

[Dependency(ReplaceServices = true)]
public class AmmenTravelBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AmmenTravelResource> _localizer;

    public AmmenTravelBrandingProvider(IStringLocalizer<AmmenTravelResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
