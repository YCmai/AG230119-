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
    /// <summary>
    /// 负责工作站超时检查
    /// </summary>
    public class OpStationOnlinTimeoutCheckWorker : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    {
        private readonly IOperationStationAppService operationStationAppService;

        public OpStationOnlinTimeoutCheckWorker(IOperationStationAppService operationStationAppService) : base(10)  //10s执行一次。
        {
            this.operationStationAppService = operationStationAppService;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await this.operationStationAppService.OpStationOnLineTimeOutCheck();
        }
    }
}
