using AmmenTravel.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AmmenTravel.Permissions
{
    public class AmmenTravelPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup(AmmenTravelPermissions.GroupName, L("Permission:AmmenTravel"));

            var destinosPermission = myGroup.AddPermission(AmmenTravelPermissions.Destinos.Default, L("Permission:Destinos"));
            destinosPermission.AddChild(AmmenTravelPermissions.Destinos.Create, L("Permission:Destinos.Create"));
            destinosPermission.AddChild(AmmenTravelPermissions.Destinos.Edit, L("Permission:Destinos.Edit"));
            destinosPermission.AddChild(AmmenTravelPermissions.Destinos.Delete, L("Permission:Destinos.Delete"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<AmmenTravelResource>(name);
        }
    }
}