using PDS.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace PDS.DbMigrator
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(PDSEntityFrameworkCoreModule),
        typeof(PDSApplicationContractsModule)
        )]
    public class PDSDbMigratorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
