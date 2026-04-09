using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;


namespace WMS.R2POutputModule
{
    public class R2POutputModule : AbpModule
    {
        ///使用的功能：HB任务的添加、查询、取消，拣货任务的添加、查询、取消。
        ///提供的功能：线边呼叫出库流程的添加、取消和查看。添加后，会通过控制多个基础任务完成整个出库流程。
        ///触发的事件：流程状态变更。
        ///关注的事件：基础任务状态变更。
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<R2POutputModule>();  
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
                options.ConventionalControllers.Create(typeof(R2POutputModule).Assembly);
            });
        }

    }
}
