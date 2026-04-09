using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.VirtualFileSystem;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace WMS.Identity
{
    [DependsOn(
        typeof(AbpIdentityDomainModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpAccountApplicationModule),
        typeof(AbpPermissionManagementDomainIdentityModule)
        )]
    [DependsOn(typeof(AbpVirtualFileSystemModule))]
    public class WMSIdentityModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<WMSIdentityModule>();
            });
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(WMSIdentityModule).Assembly);
            });
            context.Services.Replace(ServiceDescriptor.Scoped<IUserValidator<IdentityUser>, UserValidator<IdentityUser>>());
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<WMSIdentityModule>("WMS.Identity");
            });
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<WmsIdentityResource>("en")
                    .AddVirtualJson("/Domain/Localization");

                options.DefaultResourceType = typeof(WmsIdentityResource);
            });
        }
    }
}