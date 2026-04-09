using Volo.Abp.Modularity;

namespace PDS
{
    [DependsOn(
        typeof(PDSApplicationModule),
        typeof(PDSDomainTestModule)
        )]
    public class PDSApplicationTestModule : AbpModule
    {

    }
}