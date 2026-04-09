using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using WMS.BarCodeModule;
using WMS.MaterialModule.Domain.Localization;
using WMS.StorageModule;


namespace WMS.LineCallInputModule
{

    /*
     * 本模块提供了LineCallInput(Output)Order对象,只需要在仓储中添加对象，并调用启动，就可以实现呼叫入库。
     * 入库呼叫单，实际是先入库，接着产生一个入库移库单实现移库。
     * 注意：线边出库一次只出库（搬运）一个物料。
     * 注意： todo,启动时，如果没有可分配的储位，现在是报错回滚。
     */
    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    [DependsOn(typeof(AbpLocalizationModule))]
    public class WMSLineCallInputModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<WMSLineCallInputModule>();  
            });

            ConfigureConventionalControllers();

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<WMSLineCallInputModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<LineCallResources>("en")
                    .AddVirtualJson("/WMS/InputOutputModule/Domain/Localization");
            });
        }


        /// <summary>
        /// 自动生成api
        /// </summary>
        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(WMSLineCallInputModule).Assembly);
            });
        }

    }
}
