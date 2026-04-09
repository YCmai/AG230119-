using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace WMS
{
    [Dependency(ReplaceServices = true)]
    public class WMSBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "WMS";
    }
}
