using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WMS.StorageModule
{
    public class StoragePermissionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var storgePermissionGroup = context.AddGroup(StoragePermissionConsts.StorageGroupName, L("Storage.Permission.Group"));
            storgePermissionGroup.AddPermission(StoragePermissionConsts.StorageModuleName, L("Storage.Permission.Module"));
            var wareHousePermissionGroup = context.AddGroup(StoragePermissionConsts.WareHouseGroupName, L("WareHouse.Permission.Group"));
            wareHousePermissionGroup.AddPermission(StoragePermissionConsts.WareHouseModuleName, L("WareHouse.Permission.Module"));

            //permissionModule.AddChild(StoragePermissionConsts.StorageList, L("Storage.Permission.Management"));
        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<StorageResource>(name);
        }
    }
}
