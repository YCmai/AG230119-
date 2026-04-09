using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using HBTaskModule.Domain;
using HBTaskModule.Domain.Shared;
using WMS.R2POutputModule.Domain;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;

namespace WMS.R2POutputModule.Application
{
    //子任务状态更新时，需要更新。（注意：如果是更新后，那么可能会出现问题，因为子任务已经更新了，但没有更新主任务，这就导致主任务的更新不会再被触发）。
    //任务添加时，需要更新，让任务马上开始处理。

    public class R2POutputTaskTaskHandle: IParentTaskHandle<R2POutputTask>
	{
        private readonly IRepository<R2POutputTask, Guid> _lineCallInputProcessRepos;
        private readonly IRepository<HBTask_Allocation, Guid> _hBTask_AllocationRepos;
        private readonly IRepository<HBTask_Load, Guid> _hBTask_LoadRepos;
        private readonly IRepository<HBTask_UnLoad, Guid> _hBTask_UnLoadRepos;
        private readonly IRepository<HBTask_Release, Guid> _hBTask_ReleaseRepos;
        private readonly ITaskTrackService _taskTrackService;

        public R2POutputTaskTaskHandle(IRepository<R2POutputTask, Guid> lineCallInputProcessRepos, 
            IRepository<HBTask_Allocation, Guid> hBTask_AllocationRepos,
            IRepository<HBTask_Load,Guid> hBTask_LoadRepos,
            IRepository<HBTask_UnLoad,Guid> hBTask_UnLoadRepos,
            IRepository<HBTask_Release,Guid> hBTask_ReleaseRepos,
			ITaskTrackService taskTrackService)
        {
            _lineCallInputProcessRepos = lineCallInputProcessRepos;
            _hBTask_AllocationRepos = hBTask_AllocationRepos;
            _hBTask_LoadRepos = hBTask_LoadRepos;
            _hBTask_UnLoadRepos = hBTask_UnLoadRepos;
            _hBTask_ReleaseRepos = hBTask_ReleaseRepos;
            _taskTrackService= taskTrackService;
        }

        public async Task StartTask(R2POutputTask  task)
        {
            if (task.State != R2POutputTaskState.NotStart) throw new Exception("不可以重复启动");


            //状态更改
            task.SetStatus(R2POutputTaskState.AllocatingAgv);  //task不是这里查询处理的，不再这里调用更新数据库。
            //var t1 = await _lineCallInputProcessRepos.UpdateAsync(task);  //todo,预期调用这个会导致异常的，但不知道为什么，居然可以正常运行。

            //添加新的任务
            var NewAllocationTask = new HBTask_Allocation(Guid.NewGuid(), task.Id, task.GetType().FullName, "", EAgvType.AnyType, EAllcateType.AllocateWhenCatch, task.LoadNodeName);
             await _hBTask_AllocationRepos.InsertAsync(NewAllocationTask);

            //添加跟踪，这样子任务完成后会自动调用本类的更新函数。
            await _taskTrackService.AddAssociation<R2POutputTask, R2POutputTaskTaskHandle>(NewAllocationTask, task);
        }


        /// <summary>
        /// 根据基础任务，更新process的状态并操作新的基础任务。
        /// </summary>
        /// <param name="basicTask"></param>
        /// <returns></returns>
        public  async Task UpdateTaskWhenSubtaskUpdatingAsync(TaskBaseAggregateRoot basicTask)
        {
            //获取流程任务，用户更新
            var task = await _lineCallInputProcessRepos.GetAsync((Guid)basicTask.ParentId);
            switch (task.State)
            {
                case R2POutputTaskState.NotStart:
                    //1.不应该存在，因为任务启动了才会有子任务。
                    throw new Exception("不应该存在，任务还没有开始但有子任务完成。");
                    break;

                case R2POutputTaskState.AllocatingAgv:
                    //检查分配任务
                    var allcationTask = basicTask as HBTask_Allocation;
                    if (allcationTask == null) throw new Exception("非预期任务类型：" + basicTask.GetType());

                    switch(allcationTask.AllcateStatus)
                    {
                        case EAllocationStatus.Fault:
                            //todo,分配失败应该怎么定义呢？什么情况下会分配失败？
                            break;

                        case EAllocationStatus.Cancel:
                            //todo
                            break;
                        case EAllocationStatus.Success:
                            //状态更改
                            task.SetStatus(R2POutputTaskState.AgvLoading);
                            task.DeviceId = allcationTask.ActualAgvId;
                            await _lineCallInputProcessRepos.UpdateAsync(task);

                            //添加新的任务
                            var NewLoadTask = new HBTask_Load(Guid.NewGuid(), task.Id, task.GetType().FullName, task.LoadNodeName, EMaterialCheckType.NeednotCheck, "");
                            await _hBTask_LoadRepos.InsertAsync(NewLoadTask);

							//添加跟踪，这样子任务完成后会自动调用本类的更新函数。
							await _taskTrackService.AddAssociation<R2POutputTask, R2POutputTaskTaskHandle>(NewLoadTask, task);
                            break;
                        default:
                            //啥都不用干
                            break;
                    }

                    break;

                case R2POutputTaskState.AgvLoading:

                    //检查取货任务
                    var loadTask = basicTask as HBTask_Load;
                    if (loadTask == null) throw new Exception("非预期任务类型：" + basicTask.GetType());

                    switch(loadTask.LoadStatus)
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
                            task.SetStatus(R2POutputTaskState.AgvUnloading);
                            await _lineCallInputProcessRepos.UpdateAsync(task);

                            //添加新的任务
                            var NewUnloadTask = new HBTask_UnLoad(Guid.NewGuid(), task.Id, task.GetType().FullName, task.UnloadNodeName);
                            await _hBTask_UnLoadRepos.InsertAsync(NewUnloadTask);

							//添加跟踪，这样子任务完成后会自动调用本类的更新函数。

							await _taskTrackService.AddAssociation<R2POutputTask, R2POutputTaskTaskHandle>(NewUnloadTask, task);
							break;
                        default:
                            //啥都不用干
                            break;
                    }

                    break;

                case R2POutputTaskState.AgvLoadFalse:
                    //todo:
                    //1.检查是异常处理任务，如果任务已经完成，则根据选择的处理方案，修改状态分别为 a）Cancel。 b）FinishedAndClosed。 c）NotStart。需要同时添加agv释放任务。
                    break;

                case R2POutputTaskState.AgvUnloading:

                    //检查卸货任务
                    var unloadTask = basicTask as HBTask_UnLoad;
                    if (unloadTask == null) throw new Exception("非预期任务类型：" + basicTask.GetType());

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
                            task.SetStatus(R2POutputTaskState.FinishedAndClosed);
                            await _lineCallInputProcessRepos.UpdateAsync(task);

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

                case R2POutputTaskState.AgvUnloadFalse:
                    //todo:
                    //1.检查是取货异常处理任务，如果任务已经完成，则根据选择的处理方案，修改状态a）FinishedAndClosed并添加agv释放任务。
                    break;

                case R2POutputTaskState.FinishedAndClosed:
                case R2POutputTaskState.Cancel:
                    //啥都不用做。
                    break;

                default:
                    throw new Exception("未处理的任务状态：" + task.State);
                    break;
            }
        }


        
    }
}

