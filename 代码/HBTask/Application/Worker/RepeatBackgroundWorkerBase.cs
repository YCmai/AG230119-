using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace HBTaskModule.Application
{
    //测试发现：定时任务会一直产生内存知道一个阈值再逐步降下来（测试开了两个定时任务，50ms一次，内存从280M一直飙到700M才降低）
    /// <summary>
    /// 单线程定时执行
    /// </summary>
    [DisallowConcurrentExecution]  //这个特性，确保同一时间只有一个线程在执行。
    public abstract class RepeatBackgroundWorkerBase : QuartzBackgroundWorkerBase
    {
        public RepeatBackgroundWorkerBase(int sec)
        {
            JobDetail = JobBuilder.Create(this.GetType()).WithIdentity(this.GetType().Name).Build();
            Trigger = TriggerBuilder.Create()
                        .WithIdentity(this.GetType().Name)
                        .WithSimpleSchedule(s =>s.WithIntervalInSeconds(sec).RepeatForever())
                        .Build();
        }

    }
}
