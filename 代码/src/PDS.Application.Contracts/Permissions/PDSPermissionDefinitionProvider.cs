using PDS.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace PDS.Permissions
{
    public class PDSPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup(PDSPermissions.GroupName);
            //Define your own permissions here. Example:
            //myGroup.AddPermission(PDSPermissions.MyPermission1, L("Permission:MyPermission1"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<PDSResource>(name);
        }
    }
}
