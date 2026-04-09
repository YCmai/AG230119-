using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using WMS.LineCallInputModule.Domain;
using WMS.LineCallProcessTaskModule.Domain;
using WMS.MaterialModule.Domain;
using WMS.StorageModule.Domain;

namespace WMS.LineCallInputModule.Application
{

    /// <summary>
    /// 当LineCallInputTask更新时，更新对应的LineCallInputOrder
    /// </summary>
    class LineCallOutputTaskUpdatedEventHandle : ILocalEventHandler<EntityUpdatedEventData<LineCallOutputTask>>, ITransientDependency
    {
        private readonly IRepository<LineCallOutputOrder, Guid> _lineCallOutputOrderRepos;
        private readonly IRepository<LineCallOutputTask, Guid> _lineCallOutputTaskRepos;
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;
        private readonly MaterialManager _materialManager;
        private readonly IRepository<Storage, string> _storageRepos;

        public LineCallOutputTaskUpdatedEventHandle(
            IRepository<LineCallOutputOrder, Guid>  lineCallOutputOrderRepos,
            IRepository<LineCallOutputTask, Guid>  lineCallOutputTaskRepos,
            IRepository<MaterialItem,Guid> materialItemRepos,
            MaterialManager materialManager,
            IRepository<Storage, string> storageRepos)
        {
            _lineCallOutputOrderRepos = lineCallOutputOrderRepos;
            _lineCallOutputTaskRepos = lineCallOutputTaskRepos;
            _materialItemRepos = materialItemRepos;
            _materialManager = materialManager;
            _storageRepos = storageRepos;
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<LineCallOutputTask> eventData)
        {
            //获取LineCallInputOrder

            switch (eventData.Entity.State)
            {
                case LineCallOutputTaskState.Cancel:  //取消，还需要知道物料是否已经之前取货又放回去了，因为如果取货，物料信息已经被改到卸货点。
                    //todo,这个订单已经被上架了，如果移储任务被取消，订单的状态也不能回到上架前，所以只能：1.继续安排新的任务去移储。 2.让人工搬运，并提供UI接口给操作员，实现本Order的更新。  3.前面两者让客户去选。
                    break;
                case LineCallOutputTaskState.AgvUnloading:
                    //agv正在卸货，此时取货完成，可以释放取货储位了。
                    var OutputOrder = await _lineCallOutputOrderRepos.GetAsync(p=>p.LineCallOutputTaskId == eventData.Entity.Id);
                    var LoadStorage = await _storageRepos.GetAsync(OutputOrder.StoreStorageId,includeDetails:true);

                    if (LoadStorage.TryRemoveLock(OutputOrder.Id.ToString()))  //加锁是根据OrderId加锁，所有解锁也是。
                    {
                        await _storageRepos.UpdateAsync(LoadStorage); 
                    }

                    //物料信息转移到卸货点。   这是为了清空取货点的物料信息，否则物料还在取货点，无法有效利用储位。
                    var UnLoadStorage = await _storageRepos.GetAsync(OutputOrder.OutputStorageId);
                    var Material = await _materialItemRepos.GetAsync(p=> p.BarCode == OutputOrder.BarCode);
                    await _materialManager.MoveMaterialToNewStrorage(Material, UnLoadStorage);
                    await _storageRepos.UpdateAsync(UnLoadStorage);
                    await _materialItemRepos.UpdateAsync(Material);

                    break;
                case LineCallOutputTaskState.FinishedAndClosed:
                    OutputOrder = await _lineCallOutputOrderRepos.GetAsync(p => p.LineCallOutputTaskId == eventData.Entity.Id);
                    if (OutputOrder.State !=  LineCallOutputOrderState.Finished)
                    {
                        //如果取货储位没有释放，这里也需要释放。（例如上面的AgvUnloading是一个过渡态，因为网络原因，这个过渡态可能会被跳过直接进入最终态，所以取货储位就不一定被释放了。
                        LoadStorage = await _storageRepos.GetAsync(OutputOrder.StoreStorageId,includeDetails:true);
                        UnLoadStorage = await _storageRepos.GetAsync(OutputOrder.OutputStorageId, includeDetails: true);
                        LoadStorage.TryRemoveLock(OutputOrder.Id.ToString());  //加锁是根据OrderId加锁，所有解锁也是。
                        await _storageRepos.UpdateAsync(LoadStorage);
                        Material = await _materialItemRepos.GetAsync(p => p.BarCode == OutputOrder.BarCode);
                        if (Material.StorageId != UnLoadStorage.Id)
                        {
                            await _materialManager.MoveMaterialToNewStrorage(Material, UnLoadStorage);
                            await _materialItemRepos.UpdateAsync(Material);
                        }

                        //那可以释放卸货储位了
                        
                        UnLoadStorage.TryRemoveLock(OutputOrder.Id.ToString());  //加锁是根据OrderId加锁，所有解锁也是。
                        await _storageRepos.UpdateAsync(UnLoadStorage);

                        //当做物料已经出库
                        Material = await _materialItemRepos.GetAsync(p => p.BarCode == OutputOrder.BarCode);
                        await _materialManager.TakeAwayFromLock(Material, 1);
                        await _materialItemRepos.UpdateAsync(Material);

                        //更新Order状态
                        OutputOrder.SetState( LineCallOutputOrderState.Finished);
                        await _lineCallOutputOrderRepos.UpdateAsync(OutputOrder);
                    }
                    break;
                default:
                    //其它不用管。
                    break;
            }
        }
    }
}
