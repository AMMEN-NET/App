using Volo.Abp.Settings;

namespace AmmenTravel.Settings;

public class AmmenTravelSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AmmenTravelSettings.MySetting1));
    }
}
