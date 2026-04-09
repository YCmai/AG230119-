using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using WMS.StorageModule;



namespace WMS.BarCodeModule
{
    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    [DependsOn(typeof(AbpLocalizationModule))]
    public class WMSBarCodeModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<WMSBarCodeModule>();  
            });

            ConfigureConventionalControllers();

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<WMSBarCodeModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<BarcodeResource>("en")
                    .AddVirtualJson("/WMS/BarCodeModule/Domain/Localization");
            });
        }


        /// <summary>
        /// 自动生成api
        /// </summary>
        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(WMSBarCodeModule).Assembly);
            });
        }

    }
}
