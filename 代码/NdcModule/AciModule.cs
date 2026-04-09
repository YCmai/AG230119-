using AciModule.Domain.Entitys;
using AciModule.Domain.Service;
using AciModule.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;


namespace AciModule
{
    public class AciModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {               
                options.AddMaps<AciModule>();  
            });

            ConfigureConventionalControllers();  
        }


        

        /// <summary>
        /// 自动生成api
        /// </summary>
        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(AciModule).Assembly);
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {           
           context.ServiceProvider.GetService<AciAppManager>();  //这里是为了实例化这个single
        }

    }
}
