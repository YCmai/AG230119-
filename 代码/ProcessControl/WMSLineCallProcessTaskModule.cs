using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;


namespace WMS.LineCallProcessTaskModule
{

	/*
     * 本模块实现了LineCallInput（Output）Task，它是适用于线边整存整出的任务
     * 只要通过仓储添加了任务，本模块将会根据任务参数对接RCS（心跳或ndc）以执行该任务并更新任务状态
     */
	public class WMSLineCallProcessTaskModule : AbpModule
    {

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>  //这里是配置autoMapper；（猜测实现的功能是找到指定模块的所有AutoMapperProfile，并执行。
            {
                options.AddMaps<WMSLineCallProcessTaskModule>();  
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
                options.ConventionalControllers.Create(typeof(WMSLineCallProcessTaskModule).Assembly);
            });
        }

    }
}
