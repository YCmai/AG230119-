using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using WMS.MaterialModule.Domain.Localization;

namespace WMS.InputOutputModule
{
    public class LineCallPermissionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var permissionGroup = context.AddGroup(LineCallPermissionConsts.GroupName, L("LineCall.Permission.Group"));
            var permissionModule = permissionGroup.AddPermission(LineCallPermissionConsts.ModuleName, L("LineCall.Permission.Module"));
            permissionModule.AddChild(LineCallPermissionConsts.LineCallInput, L("LineCall.Permission.Input"));
        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<LineCallResources>(name);
        }
    }
}
