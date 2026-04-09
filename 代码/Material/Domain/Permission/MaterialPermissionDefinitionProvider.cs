using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using WMS.MaterialModule.Domain.Localization;

namespace WMS.MaterialModule
{
    public class MaterialPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var permissionGroup = context.AddGroup(MaterialPermissionConsts.GroupName, L("Material.Permission.Group"));
            var permissionModule = permissionGroup.AddPermission(MaterialPermissionConsts.ModuleName, L("Material.Permission.Module"));
            permissionModule.AddChild(MaterialPermissionConsts.Management, L("Material.Permission.Management"));
            permissionModule.AddChild(MaterialPermissionConsts.AddStock, L("Material.Permission.AddStock"));
            permissionModule.AddChild(MaterialPermissionConsts.ModifyUsableQuantity, L("Material.Permission.ModifyUsableQuantity"));
            permissionModule.AddChild(MaterialPermissionConsts.ModifyRcord, L("Material.Permission.ModifyRcord"));

            var permissionStock = permissionModule.AddChild(MaterialPermissionConsts.StockManagement, L("Material.Permission.Stock"));
            permissionStock.AddChild(MaterialPermissionConsts.StockOutputRecord, L("Material.Permission.Stock.Output"));
            permissionStock.AddChild(MaterialPermissionConsts.StockDetail, L("Material.Permission.Stock.Detail"));
            permissionStock.AddChild(MaterialPermissionConsts.StockStatis, L("Material.Permission.Stock.Statis"));

        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<MaterialResources>(name);
        }
    }
}
