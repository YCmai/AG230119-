using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using WMS.StorageModule;

namespace WMS.BarCodeModule
{
    public class BarcodePermissionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var permissionGroup = context.AddGroup(BarcodePermissionConsts.GroupName, L("Barcode.Permission.Group"));
            permissionGroup.AddPermission(BarcodePermissionConsts.ModuleName, L("Barcode.Permission.Module"));
        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<BarcodeResource>(name);
        }
    }
}
