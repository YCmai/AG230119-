using Volo.Abp.Settings;

namespace PDS.Settings
{
    public class PDSSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(PDSSettings.MySetting1));
        }
    }
}
