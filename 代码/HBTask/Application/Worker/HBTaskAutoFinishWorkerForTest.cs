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
using AciModule.Domain.Entitys;

namespace HBTaskModule.Application
{
    /// <summary>
    /// 定期把未完成任务同步到心跳系统，并且从HB的同步到agv上。
    /// </summary>
    public class HBTaskAutoFinishWorkerForTest : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    {
        private readonly IRepository<HBTask_Allocation, Guid> _hBTask_AllocationRepos;
        private readonly IRepository<HBTask_Load, Guid> _hBTask_LoadRepos;
        private readonly IRepository<HBTask_UnLoad, Guid> _hBTask_UnLoadRepos;
        private readonly IRepository<HBTask_Move,Guid> _hBTask_MoveRepos;
        private readonly IRepository<HBTask_Release, Guid> _hBTask_ReleaseRepos;

        public HBTaskAutoFinishWorkerForTest
            (IRepository<HBTask_Allocation,Guid> hBTask_AllocationRepos,
            IRepository<HBTask_Load,Guid> hBTask_LoadRepos,
            IRepository<HBTask_UnLoad,Guid> hBTask_UnLoadRepos,
            IRepository<HBTask_Move,Guid> hBTask_MoveRepos,
            IRepository<HBTask_Release,Guid> hBTask_ReleaseRepos) : base(5) 
        {
            _hBTask_AllocationRepos = hBTask_AllocationRepos;
            _hBTask_LoadRepos = hBTask_LoadRepos;
            _hBTask_UnLoadRepos = hBTask_UnLoadRepos;
            _hBTask_MoveRepos = hBTask_MoveRepos;
            _hBTask_ReleaseRepos = hBTask_ReleaseRepos;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            var allcationTasks = await _hBTask_AllocationRepos.GetListAsync(p => 
            p.AllcateStatus == EAllocationStatus.NotSync 
            || p.AllcateStatus == EAllocationStatus.Syncing 
            || p.AllcateStatus == EAllocationStatus.Excuting);
            if(allcationTasks.Any())
            {
                foreach (var t in allcationTasks)
                {
                    if ((DateTime.Now - t.CreationTime).TotalSeconds > 5)
                    {
                        t.ActualAgvId = "testAgv";
                        t.SetStatus(EAllocationStatus.Success);
                        await _hBTask_AllocationRepos.UpdateAsync(t);
                        System.Diagnostics.Debug.WriteLine(t.GetType().Name + "id:" + t.Id + "设置为完成");
                    }
                }
            }


            var loadTasks = await _hBTask_LoadRepos.GetListAsync(p =>
            p.LoadStatus == ELoadStatus.NotSync
            || p.LoadStatus == ELoadStatus.Syncing
            || p.LoadStatus == ELoadStatus.Excuting);
            foreach(var t in loadTasks)
            {
                if ((DateTime.Now - t.CreationTime).TotalSeconds > 5)
                {
                    t.SetStatus(ELoadStatus.Success);
                    await _hBTask_LoadRepos.UpdateAsync(t);
                    System.Diagnostics.Debug.WriteLine(t.GetType().Name + "id:" + t.Id + "设置为完成");
                }
            }

            var unloadTasks = await _hBTask_UnLoadRepos.GetListAsync(p =>
            p.UnloadStatus == EUnloadStatus.NotSync
            || p.UnloadStatus == EUnloadStatus.Syncing
            || p.UnloadStatus == EUnloadStatus.Excuting);
            foreach(var t in unloadTasks)
            {
                if ((DateTime.Now - t.CreationTime).TotalSeconds > 5)
                {
                    t.SetStatus(EUnloadStatus.Success);
                    await _hBTask_UnLoadRepos.UpdateAsync(t);
                    System.Diagnostics.Debug.WriteLine(t.GetType().Name + "id:" + t.Id + "设置为完成");
                }
            }

            var moveTasks = await _hBTask_MoveRepos.GetListAsync(p=>
            p.MoveStatus == EMoveStatus.NotSync
            || p.MoveStatus == EMoveStatus.Syncing
            || p.MoveStatus == EMoveStatus.Excuting);
            foreach(var t in moveTasks)
            {
                if ((DateTime.Now - t.CreationTime).TotalSeconds > 5)
                {
                    t.SetStatus(EMoveStatus.Success);
                    await _hBTask_MoveRepos.UpdateAsync(t);
                    System.Diagnostics.Debug.WriteLine(t.GetType().Name + "id:" + t.Id + "设置为完成");
                }
            }

            var releaseTasks = await _hBTask_ReleaseRepos.GetListAsync(p =>
            p.ReleaseStatus == EReleaseStatus.NotSync
            || p.ReleaseStatus == EReleaseStatus.Syncing);
            foreach (var t in releaseTasks)
            {
                if ((DateTime.Now - t.CreationTime).TotalSeconds > 5)
                {
                    t.SetStatus(EReleaseStatus.Success);
                    await _hBTask_ReleaseRepos.UpdateAsync(t);
                    System.Diagnostics.Debug.WriteLine(t.GetType().Name + "id:" + t.Id + "设置为完成");
                }
            }
        }

    }
}
