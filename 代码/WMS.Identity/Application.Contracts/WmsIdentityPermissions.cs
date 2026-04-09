namespace WMS.Identity
{
    public class WmsIdentityPermissions
    {
        public const string SettingGroupName = "Setting";
        public const string AuditGrouName = "Aduit";
        public class Setting
        {
            public const string Default = SettingGroupName + ".Global";
            public const string Update = Default + ".Update";
        }
        public class Audit
        {
            public const string Default = AuditGrouName + ".List";
        }
    }
}
