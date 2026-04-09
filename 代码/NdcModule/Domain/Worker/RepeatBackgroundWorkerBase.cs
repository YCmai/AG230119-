using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace AciModule.Domain.Worker
{
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
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(sec).RepeatForever())
                        .Build();
        }

    }
}
