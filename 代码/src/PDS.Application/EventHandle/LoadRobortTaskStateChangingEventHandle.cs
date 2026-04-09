using PDS.Application.Interface;
using PDS.Domain.Entitys;
using PDS.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Settings;

namespace PDS.EventHandle
{
    /// <summary>
    /// 操作站点上料设备任务状态更改事件。（处理“包裹放置”任务的结果）
    /// </summary>
    public class LoadRobortTaskStateChangingEventHandle : ILocalEventHandler<EntityStateChangingEvent<PackageLoadrobotTask, PackageLoadrobotTaskState>>, ITransientDependency
    {
        private readonly IRepository<DeliverProcessTask, string> deliverProcessRepos;
        private readonly IDeliverOutletAllocator deliverOutletAllocator;
        private readonly IRepository<HBAgvTask,string> hBDeviceTasks;
        private readonly IRepository<PackageSort, string> packageSortsRepos;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IRepository<Package, string> packagesRepos;
        private readonly ISettingProvider settingProvider;

        public LoadRobortTaskStateChangingEventHandle(
            IRepository<DeliverProcessTask, string> deliverProcessRepos,
            IDeliverOutletAllocator deliverOutletAllocator,
            IRepository<HBAgvTask, string> hBDeviceTasks,
            IRepository<PackageSort,string> packageSortsRepos,
            IRepository<DeliverOutlet,string> deliverOutletsRepos,
            IRepository<Package,string> packagesRepos,
            ISettingProvider settingProvider
            )
        {
            this.deliverProcessRepos = deliverProcessRepos;
            this.deliverOutletAllocator = deliverOutletAllocator;
            this.hBDeviceTasks = hBDeviceTasks;
            this.packageSortsRepos = packageSortsRepos;
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.packagesRepos = packagesRepos;
            this.settingProvider = settingProvider;
        }
        public async Task HandleEventAsync(EntityStateChangingEvent<PackageLoadrobotTask, PackageLoadrobotTaskState> eventData)
        {
            switch (eventData.Entity.TaskState)
            {
                case PackageLoadrobotTaskState.Finished:
                    var Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessId);
                    if (Process.DetailState == DeliverProcessState.LOADING_PACKING)  //原来的状态是正在放置包裹，否则是异常。
                    {
                        //1.确定投递口
                        var Package = await this.packagesRepos.GetAsync(Process.PackageId);
                        var PackageSort = await this.packageSortsRepos.GetAsync(Package.PackageSortId);
                        var Outlets = (await this.deliverOutletsRepos.WithDetailsAsync(p => p.HandlingProcess)).ToList();
                        var DeliverOutletForProcess = this.deliverOutletAllocator.AllocateDeliverOutlet(PackageSort, Outlets);
                        if (DeliverOutletForProcess != null)    //有适合投递口              
                        {
                            //投递口关联对应工作流
                            DeliverOutletForProcess.AddHandlingProcessLink(Process);
                            var a = deliverOutletsRepos.GetDbContext().Set<DeliverOutlet>().Local.FirstOrDefault(p => p.Id == "1");
                            await this.deliverOutletsRepos.UpdateAsync(DeliverOutletForProcess);

                            //工作流关联对应投递口和更新状态
                            Process.AllocateDeliverOutlet(DeliverOutletForProcess);
                            Process.ClaimDetailState(DeliverProcessState.AGV_DELIVERING);
                            await this.deliverProcessRepos.UpdateAsync(Process);

                            //添加HB投递任务
                            var AgvAllocationTask = HBAgvTask.CreatDeliverTask(eventData.Entity.DeliverProcessId, DeliverOutletForProcess.UppperMapNodeName, Guid.NewGuid().ToString(), Process.AgvId);
                            await hBDeviceTasks.InsertAsync(AgvAllocationTask);
                        }
                        else  //没有适合投递口
                        {
                            //更新状态
                            Process.ClaimDetailState(DeliverProcessState.IDLE_DRIVING);
                            await this.deliverProcessRepos.UpdateAsync(Process);

                            //添加HB溜车任务
                            string MoveTaskNode = await GetMoveTargetName(""); 
                            var AgvAllocationTask = HBAgvTask.CreatMoveTask(eventData.Entity.DeliverProcessId, MoveTaskNode, Guid.NewGuid().ToString(), Process.AgvId);
                            await hBDeviceTasks.InsertAsync(AgvAllocationTask);
                        }
                    }
                    else
                    {
                        //error
                    }
                    break;

                case PackageLoadrobotTaskState.ExcuteFault:
                    //todo,处理
                    break;
                case PackageLoadrobotTaskState.NotStart:
                default:
                    //不处理
                    break;
            }


            var abc = deliverOutletsRepos.GetDbContext().Set<DeliverOutlet>().Local.FirstOrDefault(p => p.Id == "1");

        }


        private async Task<string> GetMoveTargetName(string LastTargetName)
        {

            //从配置中获取节点
            var TargetsStr = await this.settingProvider.GetOrNullAsync(PDSSettingDefinitionProvider.AgvSlideNodeNames);
            var tempTargets = TargetsStr.Split(new char[] { ',', '，' });
            List<string> Targets = new List<string>();
            foreach (var t in tempTargets)
            {
                Targets.Add(t.Trim());
            }



            if (LastTargetName.IsNullOrWhiteSpace()) return Targets[0];
            var index = Targets.IndexOf(LastTargetName);
            if (index < 0) index = 0;
            else
            {
                index++;
                if (index > Targets.Count - 1)
                {
                    index = 0;
                }
            }
            return Targets[index];
        }
    }
}
