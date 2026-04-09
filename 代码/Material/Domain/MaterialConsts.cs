using Newtonsoft.Json.Serialization;
using System.ComponentModel;

namespace WMS.MaterialModule
{
    public class MaterialPermissionConsts
    {
        public const string GroupName = "Material";

        public const string ModuleName = GroupName + "." + "Module";

        public const string Management = ModuleName + ".Management";

        public const string AddStock = ModuleName + ".AddStock";

        public const string ModifyUsableQuantity = ModuleName + ".ModifyUsableQuantity";

        public const string ModifyRcord = ModuleName + ".ModifyRcord";

        public const string StockManagement = ModuleName + ".Stock";

        public const string StockDetail = StockManagement + ".Detail";

        public const string StockOutputRecord = StockManagement + ".Output";

        public const string StockStatis = StockManagement + ".Statis";
    }
}
