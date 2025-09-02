using AmmenTravel.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace AmmenTravel.Permissions;

public class AmmenTravelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AmmenTravelPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(AmmenTravelPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AmmenTravelResource>(name);
    }
}
