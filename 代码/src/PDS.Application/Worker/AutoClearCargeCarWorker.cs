using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PDS.Application.Controcts;
using PDS.Domain.Entitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace PDS.Application.Worker
{

    /// <summary>
    /// 负责分配任务的同步和更新
    /// </summary>
    public class AutoClearCargeCarWorker : RepeatBackgroundWorkerBase, Volo.Abp.DependencyInjection.ITransientDependency
    {



        private readonly ILogger<PDSApplicationModule> logger;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IDeliverOutletAppService deliverOutletAppService;
        private readonly ISettingProvider settingProvider;

        public AutoClearCargeCarWorker(
            IRepository<DeliverOutlet, string> deliverOutletsRepos, 
            IDeliverOutletAppService deliverOutletAppService,
            ISettingProvider settingProvider
            ) : base(20)
        {
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.deliverOutletAppService = deliverOutletAppService;
            this.settingProvider = settingProvider;
        }

        [UnitOfWork]  //todo,不加会报错
        public override async Task Execute(IJobExecutionContext context)
        {
            var AutoClearLockedDeliverOutletString = await this.settingProvider.GetOrNullAsync(PDSSettingDefinitionProvider.AutoClearLockedDeliverOutlet);
            if (AutoClearLockedDeliverOutletString.IsNullOrWhiteSpace()) return;

            bool temp = false;
            if (!bool.TryParse(AutoClearLockedDeliverOutletString, out temp)) return;  //无法转化，返回
            if (!temp) return;  //转化值为false，返回


            //开始清理
            var LockedList = (await this.deliverOutletsRepos.GetQueryableAsync()).Where(p => p.State == DeliverOutletState.LOCKED).ToList();
            foreach (var deliverOutlet in LockedList)
            {
                await this.deliverOutletAppService.ClearPackagesAsync(
                    new Contracts.Dtos.ClearPackagesInput()
                    {
                        FrameId = Guid.NewGuid().ToString(),
                        DeliverOutetId = deliverOutlet.Id
                    });
            }

        }

    }
}


