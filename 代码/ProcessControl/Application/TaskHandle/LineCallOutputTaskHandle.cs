
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using HBTaskModule.Domain;
using HBTaskModule.Domain.Shared;
using WMS.LineCallProcessTaskModule.Domain;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.EventBus;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp;
using AciModule.Domain.Entitys;
using Volo.Abp.Guids;

namespace WMS.LineCallProcessTaskModule.Application
{
#if UseNdc == false

    /// <summary>
    /// 实现对LineCallOutputTask的启动和更新
    /// </summary>
    public class LineCallOutputTaskHandle: IParentTaskHandle<LineCallOutputTask>, ILocalEventHandler<EntityCreatedEventData<LineCallOutputTask>>, ITransientDependency
    {
        private readonly IRepository<LineCallOutputTask,Guid> _lineCallOutputTaskRepos;
        private readonly IRepository<HBTask_Allocation, Guid> _hBTask_AllocationRepos;
        private readonly IRepository<HBTask_Load, Guid> _hBTask_LoadRepos;
        private readonly IRepository<HBTask_UnLoad, Guid> _hBTask_UnloadRepos;
        private readonly IRepository<HBTask_Release, Guid> _hBTask_ReleaseRepos;
        private readonly ITaskTrackService _taskTrackService;

        public LineCallOutputTaskHandle(
            IRepository<LineCallOutputTask, Guid> lineCallOutputTaskRepos,
            IRepository<HBTask_Allocation, Guid> hBTask_AllocationRepos,
            IRepository<HBTask_Load, Guid> hBTask_LoadRepos,
            IRepository<HBTask_UnLoad, Guid> hBTask_UnloadRepos,
            IRepository<HBTask_Release, Guid> hBTask_ReleaseRepos,
            ITaskTrackService taskTrackService)
        {
            _lineCallOutputTaskRepos = lineCallOutputTaskRepos;
            _hBTask_AllocationRepos = hBTask_AllocationRepos;
            _hBTask_LoadRepos = hBTask_LoadRepos;
            _hBTask_UnloadRepos = hBTask_UnloadRepos;
            _hBTask_ReleaseRepos = hBTask_ReleaseRepos;
            _taskTrackService = taskTrackService;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<LineCallOutputTask> eventData)
        {
            var task = eventData.Entity;
            if (task.State != LineCallOutputTaskState.NotStart) throw new Exception("不可以重复启动");
            task.SetStatus(LineCallOutputTaskState.AllocatingAgv);

            //添加新的任务
            var NewAllocationTask = new HBTask_Allocation(Guid.NewGuid(), task.Id, task.GetType().FullName, "", EAgvType.AnyType, EAllcateType.AllocateWhenCatch, task.PickNodeName);
            await _hBTask_AllocationRepos.InsertAsync(NewAllocationTask);

            //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
            await _taskTrackService.AddAssociation<LineCallOutputTask, LineCallOutputTaskHandle>(NewAllocationTask, task);
        }

 

        public async Task UpdateTaskWhenSubtaskUpdatingAsync(TaskBaseAggregateRoot subTask)
        {
            //获取流程任务，用户更新
            var task = await _lineCallOutputTaskRepos.GetAsync((Guid)subTask.ParentId);

            if (task.ProcessExcuteType == CallLineOutputTaskExcuteType.CombineToCallLineInputProcess)
            {
                //todo
                //1.根据入库process的状态，来更新本地的状态。
                //bug，目前是依赖入库任务的，并没有自己的基础任务，所有不会有基础任务更新触发进来这里。

            }

            else
            {
                switch (task.State)
                {
                    case LineCallOutputTaskState.NotStart:
                        //todo:
                        //1.不应该存在，因为任务启动了才会有子任务。
                        break;

                    case LineCallOutputTaskState.AllocatingAgv:
                        //todo:
                        //1.检查分配任务，如果任务已经完成，则添加取货任务，并把任务状态改为Loading，DeviceId修改为已分配的值。

                        //检查分配任务
                        var allcationTask = subTask as HBTask_Allocation;
                        if (allcationTask == null) throw new BusinessException("非预期任务类型：" + subTask.GetType());

                        switch (allcationTask.AllcateStatus)
                        {
                            case EAllocationStatus.Fault:
                                //todo,分配失败应该怎么定义呢？什么情况下会分配失败？
                                break;

                            case EAllocationStatus.Cancel:
                                //todo
                                break;
                            case EAllocationStatus.Success:
                                //状态更改
                                task.SetStatus(LineCallOutputTaskState.AgvLoading);
                                task.DeviceId = allcationTask.ActualAgvId;
                                await _lineCallOutputTaskRepos.UpdateAsync(task);

                                //添加新的任务
                                var NewLoadTask = new HBTask_Load(Guid.NewGuid(), task.Id, task.GetType().FullName, task.PickNodeName, EMaterialCheckType.NeednotCheck, "");
                                await _hBTask_LoadRepos.InsertAsync(NewLoadTask);

                                //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
                                await _taskTrackService.AddAssociation<LineCallOutputTask, LineCallOutputTaskHandle>(NewLoadTask, task);
                                break;
                            default:
                                //啥都不用干
                                break;
                        }
                        break;

                    case LineCallOutputTaskState.AgvLoading:
                        //检查取货任务
                        var loadTask = subTask as HBTask_Load;
                        if (loadTask == null) throw new Exception("非预期任务类型：" + subTask.GetType());

                        switch (loadTask.LoadStatus)
                        {
                            case ELoadStatus.Cancel:
                                //todo
                                break;
                            case ELoadStatus.Fault:
                                //如果任务失败，则添加异常处理任务，并修改状态为“AgvLodaFalse”。
                                //PS：这个异常最终会提醒到客户来选择执行方案：a）取消整个任务。 b） 手动搬运到目的地并拣货完成。 c）重新安排agv。 客户先执行，再确定，表示已经按方案执行完了。
                                //todo
                                break;
                            case ELoadStatus.Success:
                                //状态更改
                                task.SetStatus(LineCallOutputTaskState.AgvUnloading);
                                await _lineCallOutputTaskRepos.UpdateAsync(task);

                                //添加新的任务
                                var NewUnloadTask = new HBTask_UnLoad(Guid.NewGuid(), task.Id, task.GetType().FullName, task.UnloadNodeName);
                                await _hBTask_UnloadRepos.InsertAsync(NewUnloadTask);

                                //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
                                await _taskTrackService.AddAssociation<LineCallOutputTask,LineCallOutputTaskHandle>(NewUnloadTask,task);
                                break;
                            default:
                                //啥都不用干
                                break;
                        }
                        break;

                    case LineCallOutputTaskState.AgvLoadFalse:
                        //todo:
                        //1.检查是异常处理任务，如果任务已经完成，则根据选择的处理方案，修改状态分别为 a）Cancel。 b）FinishedAndClosed。 c）NotStart。需要同时添加agv释放任务。
                        break;

                    case LineCallOutputTaskState.AgvUnloading:
                        //todo：
                        //1.检查是卸货任务，如果已经完成，则更状态为“UnloadedAndWaitPick"，并添加“拣货任务”（如果需要确认拣货）。或者更改状态为“FinishedAndClosed"，并添加释放agv任务。
                        //2.如果任务失败，则添加异常处理任务，并修改状态为“AgvUnloadFalse”。PS：这个异常最终会提示到客户来选择执行方案： a） 手动搬运到目的地并拣货完成。

                        //检查卸货任务
                        var unloadTask = subTask as HBTask_UnLoad;
                        if (unloadTask == null) throw new Exception("非预期任务类型：" + subTask.GetType());

                        switch (unloadTask.UnloadStatus)
                        {
                            case EUnloadStatus.Cancel:
                                //todo
                                break;
                            case EUnloadStatus.Fault:
                                //todo
                                //如果任务失败，则添加异常处理任务，并修改状态为“AgvUnloadFalse”。PS：这个异常最终会提示到客户来选择执行方案： a） 手动搬运到目的地完成。
                                break;
                            case EUnloadStatus.Success:
                                //状态更改
                                task.SetStatus(LineCallOutputTaskState.FinishedAndClosed);
                                await _lineCallOutputTaskRepos.UpdateAsync(task);

                                //添加新的任务
                                var NewReleaseTask = new HBTask_Release(Guid.NewGuid(), task.Id, task.GetType().FullName);
                                await _hBTask_ReleaseRepos.InsertAsync(NewReleaseTask);

                                //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
                                //await _taskTracks.InsertAsync(new TaskTrack(NewReleaseTask.Id, task.Id, task.GetType(), this.GetType()));  //这里不再跟踪了。
                                break;
                            default:
                                //啥都不用做
                                break;
                        }


                        break;

                    case LineCallOutputTaskState.AgvUnloadFalse:
                        //todo:
                        //1.检查是取货异常处理任务，如果任务已经完成，则根据选择的处理方案，修改状态a）FinishedAndClosed并添加agv释放任务。
                        break;

                    case LineCallOutputTaskState.UnloadedAndWaitPick:
                        //todo:
                        //1.检查拣货任务。如果已经完成，则添加释放agv任务，并把任务状态改为“FinishedAndClosed”。
                        break;

                    case LineCallOutputTaskState.FinishedAndClosed:
                    case LineCallOutputTaskState.Cancel:
                        //啥都不用做。
                        break;
                }
            }
        }
    }

#else
    /// <summary>
    /// 实现对LineCallOutputTask的启动和更新
    /// </summary>
    public class LineCallOutputTaskHandleNdc : ITaskHandle<LineCallInputTask>, ILocalEventHandler<EntityCreatedEventData<LineCallOutputTask>>, ITransientDependency
    {
        private readonly IRepository<LineCallOutputTask, Guid> _lineCallOutputTaskRepos;
        private readonly IRepository<NdcTask_Moves, Guid> _ndcTaskRepos;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IRepository<TaskTrackInfo> _taskTracks;

        public LineCallOutputTaskHandleNdc(
            IRepository<LineCallOutputTask, Guid> lineCallOutputTaskRepos,
            IRepository<NdcTask_Moves, Guid> ndcTaskRepos,
            IGuidGenerator guidGenerator,
            IRepository<TaskTrackInfo> taskTracks)
        {
            _lineCallOutputTaskRepos = lineCallOutputTaskRepos;
            _ndcTaskRepos = ndcTaskRepos;
            _guidGenerator = guidGenerator;
            _taskTracks = taskTracks;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<LineCallOutputTask> eventData)
        {
            var task = eventData.Entity;
            if (task.State != LineCallOutputTaskState.NotStart) throw new Exception("不可以重复启动");
            task.SetStatus(LineCallOutputTaskState.AllocatingAgv);

            //添加新的任务
            //添加新的任务

            var NewNDCTask = new NdcTask_Moves(_guidGenerator.Create(), task.Id, task.GetType().FullName, 0, "", AciModule.Domain.Shared.TaskTypeEnum.In,
                group: eventData.Entity.WareHouseId,
                feedArea: "",  //todo
                dischargeArea: "", //todo
                pickupSite: int.Parse(eventData.Entity.PickNodeName),   //todo
                pickupHeight: eventData.Entity.PickHeight,
                unloadSite: int.Parse(eventData.Entity.UnloadNodeName),  //todo
                unloadHeight: eventData.Entity.UnloadHeight,
                priority: 0);
            await _ndcTaskRepos.InsertAsync(NewNDCTask);

            //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
            await _taskTracks.InsertAsync(new TaskTrackInfo(NewNDCTask.Id, task.Id, task.GetType(), this.GetType()));
        }



        public async Task UpdateTaskWhenSubtaskUpdatingAsync(TaskBaseAggregateRoot basicTask)
        {
            //获取流程任务，用户更新
            var task = await _lineCallOutputTaskRepos.GetAsync((Guid)basicTask.ParentId);

            if (task.ProcessExcuteType == CallLineOutputTaskExcuteType.CombineToCallLineInputProcess)
            {
                //todo
                //1.根据入库process的状态，来更新本地的状态。
                //bug，目前是依赖入库任务的，并没有自己的基础任务，所有不会有基础任务更新触发进来这里。

            }

            else
            {

                //获取流程任务，用户更新

                var NdcTask = basicTask as NdcTask_Moves;

                switch (NdcTask.TaskStatus)
                {
                    //确认agv执行。
                    case AciModule.Domain.Shared.TaskStatuEnum.ConfirmCar:
                        if (task.State ==  LineCallOutputTaskState.AllocatingAgv)
                        {
                            task.DeviceId = NdcTask.AgvId.ToString();
                            task.SetStatus( LineCallOutputTaskState.AgvLoading);
                            await _lineCallOutputTaskRepos.UpdateAsync(task);
                        }
                        break;

                    case AciModule.Domain.Shared.TaskStatuEnum.OrderAgvFinish: //卸货失败，洗车完成,todo，假设洗车到目标卸货点
                    case AciModule.Domain.Shared.TaskStatuEnum.CanceledWashFinish: //取货后取消，触发洗车，洗车完成,假设洗车到目标卸货点
                                                                                   //取货完成
                    case AciModule.Domain.Shared.TaskStatuEnum.PickDown:
                        if (task.State == LineCallOutputTaskState.AllocatingAgv
                         || task.State == LineCallOutputTaskState.AgvLoading)
                        {
                            task.DeviceId = NdcTask.AgvId.ToString();
                            task.SetStatus(LineCallOutputTaskState.AgvUnloading);
                            await _lineCallOutputTaskRepos.UpdateAsync(task);
                        }
                        break;

                    //任务正常完成
                    case AciModule.Domain.Shared.TaskStatuEnum.TaskFinish:
                        if (task.State != LineCallOutputTaskState.FinishedAndClosed)
                        {
                            task.DeviceId = NdcTask.AgvId.ToString();
                            task.SetStatus(LineCallOutputTaskState.FinishedAndClosed);
                            await _lineCallOutputTaskRepos.UpdateAsync(task);
                        }
                        break;

                    //没有取货就取消
                    case AciModule.Domain.Shared.TaskStatuEnum.Canceled:
                    //无效取货点，任务并没有开始，任务取消。
                    case AciModule.Domain.Shared.TaskStatuEnum.InvalidUp:
                    //无效卸货点，任务并没有开始，任务取消。
                    case AciModule.Domain.Shared.TaskStatuEnum.InvalidDown:
                        task.SetStatus(LineCallOutputTaskState.Cancel);
                        await _lineCallOutputTaskRepos.UpdateAsync(task);
                        break;

                }
            }
        }
    }
#endif
}
