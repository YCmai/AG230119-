using NUglify;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using WMS.MaterialModule.Domain.Localization;

namespace WMS.MaterialModule
{
    /*
     * 本模块实现了物料的管理，使用MaterialManager可实现物料的上架，移储，出库，锁定（解锁），冻结，条码拆分，系统会自动统计物料的总数以及储位物料数量。
     * 注意：调用出库时，如果出库数量不等于条码对应的物料数量，就会自动进行条码拆分，新条码会出库，旧条码留库。
     * 注意：如果每一次搬运只能搬运条码的一部分物料，那么应该先拆分再搬运，因为搬运过程会牵涉到物料的移储，
     * 而调用移储是整个条码移储的，这很容把未搬运的物料也移储了。
     */

    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    [DependsOn(typeof(AbpLocalizationModule))]
    public class WMSMaterialModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<WMSMaterialModule>();
            });

            ConfigureConventionalControllers();

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<WMSMaterialModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<MaterialResources>("en")
                    .AddVirtualJson("/WMS/MaterialModule/Domain/Localization");
            });
        }


        /// <summary>
        /// 自动生成api
        /// </summary>
        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(WMSMaterialModule).Assembly);
            });
        }

    }
}
