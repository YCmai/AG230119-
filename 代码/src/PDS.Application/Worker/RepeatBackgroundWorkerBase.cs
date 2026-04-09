using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace PDS.Application.Worker
{
    /// <summary>
    /// 单线程执行
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
