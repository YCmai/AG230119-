using TaskBaseModule.Application;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

/*
 * 本模块实现的功能：提供绑定任务的父子关系的方法，绑定后，一旦子任务更新的时候会调用指定的类（继承IParentTaskHandle<ParentTaskT>）中指定函数。
 * 定义了TaskBaseAggregateRoot，所有任务必须继承这个基类。
 * 定义了IParentTaskHandle<ParentTaskT>，父任务应该继承这个接口。它描述父任务的更新逻辑。
 * 定义了ITaskTrackService，提供了绑定的方法。
 */


namespace TaskBaseModule
{
    
	public class TaskBaseModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<TaskBaseModule>();  
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
                options.ConventionalControllers.Create(typeof(TaskBaseModule).Assembly);
            });
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {

            var abc = context.ServiceProvider.GetService(typeof(TaskHandleTypeMap));
            (abc as TaskHandleTypeMap)?.InialMap();
            base.OnPreApplicationInitialization(context);
        }

	}
}
