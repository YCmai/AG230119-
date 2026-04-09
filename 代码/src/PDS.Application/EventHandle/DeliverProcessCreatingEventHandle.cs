using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;

namespace PDS.EventHandle
{

    public class DeliverProcessCreatingEventHandle : ILocalEventHandler<EntityCreatingEventData<DeliverProcessTask>>,ITransientDependency
    {
        private readonly IRepository<HBAgvTask,string> hBDeviceTasksRepos;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IRepository<OperationStation, string> operationStationsRepos;

        public DeliverProcessCreatingEventHandle(
            IRepository<HBAgvTask,string> hBDeviceTasksRepos,
            IRepository<DeliverOutlet,string> deliverOutletsRepos,
            IRepository<OperationStation,string> operationStationsRepos
            )
        {
            this.hBDeviceTasksRepos = hBDeviceTasksRepos;
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.operationStationsRepos = operationStationsRepos;
        }

        //流程创建时，同步添加“设备分配任务”，作为流程的第一步。
        public async Task HandleEventAsync(EntityCreatingEventData<DeliverProcessTask> eventData)
        {
            var OperationStation = await this.operationStationsRepos.GetAsync(eventData.Entity.OperationStationId);
            var AgvAllocationTask = HBAgvTask.CreatAllocationTask(eventData.Entity.Id, OperationStation.MapNodeName, Guid.NewGuid().ToString());
            await hBDeviceTasksRepos.InsertAsync(AgvAllocationTask);  
        }
    }
}
