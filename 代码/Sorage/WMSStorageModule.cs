using Microsoft.EntityFrameworkCore;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using WMS.StorageModule.Domain;

namespace WMS.StorageModule
{
    /*
     * 本模块实现了仓库（包含多个区域）和储位。分别提供增删改查接口，以及储位移动等功能。
     * 1.储位可以具有父子关系。区分不同的储位功能可以有多种方式：按“仓库所在区域（需要也有Category）”，按“储位的Category”，按“父储位”，这取决于应用层的使用。
     * 2.注意储位对象有Locks和CurrentNodeMaterialCount并不是储位应用的属性，但为了应用层使用方便而添加的，应用层应该管控好这两个属性。
     */
    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    [DependsOn(typeof(AbpLocalizationModule))]
    public class WMSStorageModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<WMSStorageModule>();  
            });

            Configure<AbpEntityOptions>(options =>
            {
                options.Entity<WareHouse>(a => a.DefaultWithDetailsFunc = query => query.Include(o => o.Areas));
                options.Entity<Storage>(a => a.DefaultWithDetailsFunc = query => query.Include(o => o.Locks));
            });

            ConfigureConventionalControllers();

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<WMSStorageModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<StorageResource>("en")
                    .AddVirtualJson("/WMS/StorageModule/Domain/Localizations");
            });
        }


        /// <summary>
        /// 自动生成api
        /// </summary>
        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(WMSStorageModule).Assembly);
            });
        }

    }
}
