using Newtonsoft.Json;
using PDS.Application.Controcts;
using PDS.Domain.Entitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace PDS.Application.Worker
{
    /// <summary>
    /// 负责检查笼车和投递口是否在一个位置，并尝试绑定。
    /// </summary>
    public class AutoBindingDeliverOutletAndCageCarWorker : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    {

        const string HeartBeatUrl = "http://10.10.40.31:7005/api/PDS/HeartBeat";
        private readonly IDeliverOutletAppService deliverOutletAppService;

        public AutoBindingDeliverOutletAndCageCarWorker(IDeliverOutletAppService deliverOutletAppService) : base(5)  //10s执行一次。
        {
            this.deliverOutletAppService = deliverOutletAppService;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await this.deliverOutletAppService.AutoBindingNearCageCar();
        }


    }



}
