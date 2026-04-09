using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace PDS
{
    public class PDSSettingDefinitionProvider : SettingDefinitionProvider
    {
        public const string HBUrl = "HBUrl";
        public const string AgvSlideNodeNames = "AgvSlideNodeNames";  //值用逗号隔开
        public const string AutoClearLockedDeliverOutlet = "AutoClearLockedDeliverOutlet";
        public override void Define(ISettingDefinitionContext context)
        {
            context.Add(new SettingDefinition(HBUrl,""));
            context.Add(new SettingDefinition(AgvSlideNodeNames,""));
            context.Add(new SettingDefinition(AutoClearLockedDeliverOutlet,""));
        }
    }
}
