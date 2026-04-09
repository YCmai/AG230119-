using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;


namespace HBTaskModule
{
    ///使用的功能：无。
    ///提供的功能：HB任务的添加，取消功能，查看。添加的任务，会自动同步到HB系统，并根据HB的系统进行状态的更新。
    ///触发的事件：当任务状态有变化时，会触发任务状态更改事件。
    ///关注的事件：无
    [DependsOn(typeof(TaskBaseModule.TaskBaseModule))]
    [DependsOn(typeof(AbpBackgroundWorkersQuartzModule))]  //使用了定时组件，因此加上去。
    public class HBTaskModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<HBTaskModule>();  
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
                options.ConventionalControllers.Create(typeof(HBTaskModule).Assembly);
            });
        }

    }
}
