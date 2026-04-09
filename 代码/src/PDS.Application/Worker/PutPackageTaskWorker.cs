using PDS.Application.Controcts;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace PDS.Application.Worker
{


    ///// <summary>
    ///// 负责放置任务的同步和更新
    ///// </summary>
    //public class PutPackageTaskWorker : QuartzBackgroundWorkerBase, Volo.Abp.DependencyInjection.ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    //{
    //    private readonly IServiceScopeFactory serviceScopeFactory;

    //    public PutPackageTaskWorker(IServiceScopeFactory serviceScopeFactory )  
    //    {
    //        JobDetail = JobBuilder.Create<PutPackageTaskWorker>().WithIdentity(nameof(PutPackageTaskWorker)).Build();  //todo：这些太长了，我们大部分是定时无限次运行的情况，后续可以考虑再封装一次。
    //        Trigger = TriggerBuilder.Create()
    //            .WithIdentity(nameof(PutPackageTaskWorker))
    //            .WithSimpleSchedule(s => s.WithIntervalInSeconds(2).RepeatForever())  //调度规则是每10s一次，不停循环
    //                                                                                  //.WithMisfireHandlingInstructionIgnoreMisfires()  //错过执行次数也补运行
    //            .Build();
    //        this.serviceScopeFactory = serviceScopeFactory;
    //    }

    //    [UnitOfWork]  //totest:不知道有没有效果；
    //    public override Task Execute(IJobExecutionContext context)
    //    {
    //        using (var scope = this.serviceScopeFactory.CreateScope())
    //        {
    //            scope.ServiceProvider.GetService<IRepository<DeliverOutlet, string>>();
    //            return Task.CompletedTask;
    //        }
    //    }
    //}
}
