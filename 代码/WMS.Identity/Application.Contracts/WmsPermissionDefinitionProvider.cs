using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WMS.Identity
{
    public class WmsPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            //全局变量
            var globaSettingPermissionGroup = context.AddGroup(WmsIdentityPermissions.SettingGroupName, L("Permission:Setting"));
            var globalSettingPermission = globaSettingPermissionGroup.AddPermission(WmsIdentityPermissions.Setting.Default, L("Permission:GlobalSetting"));
            globalSettingPermission.AddChild(WmsIdentityPermissions.Setting.Update, L("Permission:Update"));
            //审计日志
            var aduitGroup = context.AddGroup(WmsIdentityPermissions.AuditGrouName, L("Permission:Aduit.Group"));
            aduitGroup.AddPermission(WmsIdentityPermissions.Audit.Default, L("Permission:Aduit.Group.List"));
        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<WmsIdentityResource>(name);
        }
    }
}
