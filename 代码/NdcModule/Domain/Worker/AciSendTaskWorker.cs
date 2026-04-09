using AciModule.Domain.Entitys;
//using AciModule.Domain.Queue;
using AciModule.Domain.Service;
using AciModule.Domain.Shared;

using Quartz;

using System.Linq.Dynamic.Core;

using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace AciModule.Domain.Worker
{
    public class AciSendTaskWorker : RepeatBackgroundWorkerBase, ITransientDependency
    {
        static object LockObjct = new object();
        private readonly AciAppManager _aciAppServer;
        private readonly IRepository<NdcTask_Moves> _ndcTask;
        private readonly IGroupMaxTaskCountCategory _groupMaxTaskCountCategory;

        public AciSendTaskWorker(AciAppManager aciAppServer, IRepository<NdcTask_Moves> NdcTask, IGroupMaxTaskCountCategory groupMaxTaskCountCategory) : base(5)
        {
            _aciAppServer = aciAppServer;
            _ndcTask = NdcTask;
            _groupMaxTaskCountCategory = groupMaxTaskCountCategory;
        }
        //[UnitOfWork]
        public override async Task Execute(IJobExecutionContext context)
        {
            if (_aciAppServer.AciClient.Connected)
            {
                //lock (LockObjct)
                //{
                //获取等待分配Id的所有任务。
                var waitList = await _ndcTask.GetListAsync(i => i.TaskStatus == TaskStatuEnum.None && i.NdcTaskId == (int)TaskState.Wait);
                if (waitList.Count != 0) //如果等于0，就直接返回，导致会有存在正常分配索引的任务未能下发出去
                {
                    var WaitGroups = waitList.GroupBy(p => p.Group);

                    //获取所有已分配的ID
                    var IdHasExecution = (await _ndcTask.GetListAsync(p => p.NdcTaskId != (int)TaskState.Recycled && p.NdcTaskId != (int)TaskState.Wait))//新增
                        .Select(i => i.NdcTaskId).ToList();
                    foreach (var g in WaitGroups)
                    {
                        //id不等于负1,且不是0，则等于执行中的任务，如果执行中的大于设置的值，则不触发任务
                        var IdHasUse = (await _ndcTask.GetListAsync(p => p.NdcTaskId != (int)TaskState.Recycled && p.NdcTaskId != (int)TaskState.Wait && p.Group == g.Key)).Select(i => i.NdcTaskId).ToList();
                        //获取当前组的任务
                        var WaitList = g.OrderByDescending(p => p.Priority).ToList(); //按优先级排序，优先级高的放在前面
                        foreach (var item in WaitList)
                        {
                            var Number = _groupMaxTaskCountCategory.GetMaxTaskCount(item.Group);
                            //如果执行中的小于设置的值
                            if (IdHasUse.Count >= Number)
                            {
                                var recoveryNDCTaskid = await _ndcTask.GetListAsync(i =>
                                i.TaskStatus == TaskStatuEnum.TaskFinish ||
                                i.TaskStatus == TaskStatuEnum.Canceled ||
                                i.TaskStatus == TaskStatuEnum.InvalidUp ||
                                i.TaskStatus == TaskStatuEnum.InvalidDown ||
                                i.TaskStatus == TaskStatuEnum.CanceledWashFinish ||
                                i.TaskStatus == TaskStatuEnum.RedirectRequest ||
                                i.TaskStatus == TaskStatuEnum.OrderAgvFinish && i.NdcTaskId != -1);
                                foreach (var i in recoveryNDCTaskid)
                                {
                                    i.RecoveryId();
                                }
                                await _ndcTask.UpdateManyAsync(recoveryNDCTaskid, true);
                                break;
                            }
                            var NewId = GetRandom.getIds(IdHasExecution, 1, 10000);
                            if (NewId == 0) break;
                            item.SetNdcId(NewId);
                            await _ndcTask.UpdateAsync(item, true);
                            IdHasUse.Add(NewId);
                            IdHasExecution.Add(NewId);
                        }
                    }

                }
                //发送已分配Id但没有收到NDC回复的任务//这里表明任务还没有得到答复
                var tasks = (await _ndcTask.GetListAsync(i => i.TaskStatus == TaskStatuEnum.None && i.NdcTaskId != (int)TaskState.Wait && i.NdcTaskId != (int)TaskState.Recycled))
                   .OrderBy(i => i.Priority).Select(it => new
                   {
                       it.Id,
                       it.NdcTaskId,
                       it.TaskStatus,
                       it.Priority,
                       it.PickupSite,
                       it.UnloadSite,
                       it.Group
                   }).ToList();
                foreach (var item in tasks)
                {
                    List<int> values = new List<int>
                        {
                            item.NdcTaskId,
                            item.PickupSite,
                            item.UnloadSite
                        };
                    _aciAppServer.SendOrderInitial(null, item.NdcTaskId, 1, item.Priority, values.ToArray());
                    Thread.Sleep(1000);
                }

                //发送需要取消的任务（需要等发送完任务给NDC再进行取消，避免出现任务不同步问题）
                var cancelList = await _ndcTask.GetListAsync(i => i.CancelTask && i.TaskStatus > TaskStatuEnum.CarWash && i.TaskStatus < TaskStatuEnum.TaskFinish && !string.IsNullOrEmpty(i.SchedulTaskNo));
                if (cancelList != null && cancelList.Count > 0)
                {
                    foreach (var cancelTask in cancelList)
                    {
                        var aciCommandData = _aciAppServer.SendOrderDeleteViaOrder(null, cancelTask.OrderIndex);

                        Thread.Sleep(1000);
                    }
                }
            }
        }


    }
}
