using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using HBTaskModule.Domain;

namespace HBTaskModule.Application
{
    /// <summary>
    /// 定期把未完成任务同步到心跳系统，并且从HB的同步到agv上。
    /// </summary>
    public class HBTaskSyncWorker : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    {
        private readonly IRepository<HBTask_Allocation, Guid> _hBTask_AllocationRepos;
        private readonly IRepository<HBTask_Move> _hBTask_MoveRepos;

        public HBTaskSyncWorker(IRepository<HBTask_Allocation,Guid> hBTask_AllocationRepos,
            IRepository<HBTask_Move> hBTask_MoveRepos
            ) : base(5) 
        {
            _hBTask_AllocationRepos = hBTask_AllocationRepos;
            _hBTask_MoveRepos = hBTask_MoveRepos;
        }

        public override async Task Execute(IJobExecutionContext context)
        {


            //TODO:
            //1.获取没有确定同步的在执行的流程，调用HB的添加api以执行同步。（注意，同步状态应该有：未开始，同步中，已同步。如果需要严谨来考虑，未开始和同步中这两种状态切换是比较复杂的）。 

            //2.获取已经确认同步的在执行的流程，调用HB的任务查询查询api。
            //注意：如果一个任务调用一次接口，这里可能会调用很多次api，可能会引起系统的性能问题，
            //优化方向1：调用一次api可以同时查询多个任务的状态。（需要hb配合提供对应接口）
            //优化方向2：可以直接取消定时查询任务的功能，完全使用心跳的任务状态推送功能。（需要hb提供一个主动推送任务状态的接口，而且这个推送必须是确保推送成功才停止推送，否则会定时重新推送）
            //优化方向3：设置前面一段时间内不需要查询（这个是因为一般的任务是有一个时间的，例如1分钟，那么前一分钟就不要定时查询了，这样也可以减小查询的频次，不过需要具体问题具体分析）。
        }

    }
}
