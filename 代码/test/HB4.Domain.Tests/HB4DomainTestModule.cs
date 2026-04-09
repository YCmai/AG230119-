using PDS.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace PDS
{
    [DependsOn(
        typeof(PDSEntityFrameworkCoreTestModule)
        )]
    public class PDSDomainTestModule : AbpModule
    {

    }
}