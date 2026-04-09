using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using WMS.StorageModule.Domain.Shared;
using WMS.LineCallProcessTaskModule.Domain;
using WMS.StorageModule.Domain;
using WMS.LineCallInputModule.Application;
using Volo.Abp.DependencyInjection;
using WMS.LineCallProcessTaskModule.Application;
using Volo.Abp;
using WMS.MaterialModule.Domain;
using TaskBaseModule.Domain.Shared;
using AciModule.Domain.Entitys;
using Volo.Abp.Guids;

namespace WMS.LineCallInputModule.Domain
{

    /// <summary>
    /// 实现对LineCallInputOrder的启动移储。
    /// </summary>
    public class LineCallInputOrderManager : DomainService
    {
        private readonly IInputStoragePolicy _inputStoragePolicy;
        private readonly IOutputStoragePolicy _outputStoragePolicy;
        private readonly MaterialManager _materialManager;
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;
        private readonly IRepository<LineCallInputTask> _lineCallInputTaskRepos;
        private readonly IRepository<LineCallOutputTask, Guid> _lineCallOutputTaskRepos;
        private readonly IRepository<NdcTask_Moves> _ndcTaskRepos;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IRepository<Storage, string> _storageRepos;

        public LineCallInputOrderManager(
            IInputStoragePolicy inputStoragePolicy,
            IOutputStoragePolicy outputStoragePolicy,
            MaterialManager materialManager,
            IRepository<MaterialItem,Guid> materialItemRepos,
            IRepository<LineCallInputTask,Guid> lineCallInputTaskRepos,
            IRepository<LineCallOutputTask,Guid> lineCallOutputTaskRepos,
            IRepository<NdcTask_Moves> ndcTaskRepos,
            IGuidGenerator guidGenerator,
            IRepository<Storage,string> storageRepos)
        {
            _inputStoragePolicy = inputStoragePolicy;
            _outputStoragePolicy = outputStoragePolicy;
            _materialManager = materialManager;
            _materialItemRepos = materialItemRepos;
            _lineCallInputTaskRepos = lineCallInputTaskRepos;
            _lineCallOutputTaskRepos = lineCallOutputTaskRepos;
            _ndcTaskRepos = ndcTaskRepos;
            _guidGenerator = guidGenerator;
            _storageRepos = storageRepos;
        }

        /// <summary>
        /// 启动状态为上架的线边呼叫。订单状态必须为HasPutOn。
        /// </summary>
        /// <param name="orderToStart"></param>
        public async Task StartDeliverAsync(LineCallInputOrder orderToStart)
        {
            if (orderToStart.State != LineCallInputOrderState.HasPutOn)
                throw new BusinessException("任务已经启动。");

            //决定卸货点 //todo,如果储位满了也没有卸货点，以后应该定时更新。
            var UnloadStorage = await _inputStoragePolicy.DecideStorageToDeliver(orderToStart);
            if (UnloadStorage == null)
            {
                throw new BusinessException("卸货点无法确定");
                //throw new Exception("卸货点无法确定");
            }

            //锁定储位。
            var LoadStorage = await _storageRepos.GetAsync(orderToStart.InputStorageId, includeDetails: true);
            LoadStorage.AddTaskLock(orderToStart.Id.ToString(), StorageLockType.MaterialOut);  //对于被举升的储位，是迁出锁
            await _storageRepos.UpdateAsync(LoadStorage);

            UnloadStorage.AddTaskLock(orderToStart.Id.ToString(), StorageLockType.MaterialIn);  //对于卸货储位，是迁入锁
            await _storageRepos.UpdateAsync(UnloadStorage);

            //创建任务 

            var NewTask = new LineCallInputTask(_guidGenerator.Create(), LoadStorage.MapNodeName, LoadStorage.StartHeight, UnloadStorage.MapNodeName, UnloadStorage.StartHeight,UnloadStorage.WareHouseId);
            await _lineCallInputTaskRepos.InsertAsync(NewTask);


            //当前order关联到任务
            orderToStart.SetState(LineCallInputOrderState.Delivering);
            orderToStart.StoreStorageId = UnloadStorage.Id;
            orderToStart.BindingTask(NewTask.Id);

        }

        public async Task StartOutputAsync(LineCallOutputOrder orderToStart)
        {
            if (orderToStart.State !=  LineCallOutputOrderState.Notstart)
                throw new BusinessException("任务已经启动。");

            //决定取货物料 //todo,如果没有对应物料，以后要定期查找。
            var outputMaterial = await _outputStoragePolicy.DecideMaterialToOutput(orderToStart);
            if (outputMaterial == null) throw new BusinessException("出库物料无法确定");
            //如果获取的条码的数量大于1，那么拆分一个，拆分出来新的条码（item）进行出库。这是因为线边出库只能搬运一件物料。
            if (outputMaterial.SumQuatity > 1)
            {
				outputMaterial = await _materialManager.SplitInTwoItem(outputMaterial, 1, true);
            }
            await _materialManager.AvaileTransformToLock(outputMaterial, 1); //物料从可用转为锁定
            await _materialItemRepos.UpdateAsync(outputMaterial); 

            //创建任务
            var LoadStorage = await _storageRepos.GetAsync(outputMaterial.StorageId, includeDetails: true);
            var UnloadStorage = await _storageRepos.GetAsync(orderToStart.OutputStorageId, includeDetails: true);
            var NewTask = new LineCallOutputTask(Guid.NewGuid(), LoadStorage.MapNodeName,UnloadStorage.MapNodeName);
            await _lineCallOutputTaskRepos.InsertAsync(NewTask);

            //锁定储位。
            LoadStorage.AddTaskLock(orderToStart.Id.ToString(), StorageLockType.MaterialOut);  //对于被举升的储位，是迁出锁
            await _storageRepos.UpdateAsync(LoadStorage);

            UnloadStorage.AddTaskLock(orderToStart.Id.ToString(), StorageLockType.MaterialIn);  //对于卸货储位，是迁入锁
            await _storageRepos.UpdateAsync(UnloadStorage);

            //当前order关联到任务和储位以及物料
            orderToStart.SetState(LineCallOutputOrderState.Delivering);  
            orderToStart.ComfirmStoreStorageId(outputMaterial.StorageId);
            orderToStart.BarCode = outputMaterial.BarCode;
            orderToStart.BindingTask(NewTask.Id);

        }
    }
}
