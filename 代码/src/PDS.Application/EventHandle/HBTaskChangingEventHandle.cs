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
    /// 心跳系统任务状态更改事件。（处理“agv”任务的结果）
    /// </summary>
    public class HBTaskChangingEventHandle : ILocalEventHandler<EntityStateChangingEvent<HBAgvTask, HBAgvTaskState>>, ITransientDependency
    {
        private readonly IRepository<DeliverProcessTask, string> deliverProcessRepos;
        private readonly IRepository<PackageLoadrobotTask, string> loadrobotTaskRepos;
        private readonly IRepository<HBAgvTask, string> hBAgvTasksRepos;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IRepository<OperationStation, string> operationStationsRepos;
        private readonly IRepository<CageCar, string> cageCarsRepos;
        private readonly IRepository<Package, string> packagesRepos;
        private readonly IRepository<PackageSort, string> packageSortsRepos;
        private readonly IDeliverOutletAllocator deliverOutletAllocator;
        private readonly ISettingProvider settingProvider;

        public HBTaskChangingEventHandle(
            IRepository<DeliverProcessTask, string> deliverProcessContextsRepository,
            IRepository<PackageLoadrobotTask, string> opStationDeviceTasksRepository,
            IRepository<HBAgvTask, string> hBAgvTasksRepos,
            IRepository<DeliverOutlet,string> deliverOutletsRepos,
            IRepository<OperationStation,string> operationStationsRepos,
            IRepository<CageCar,string> cageCarsRepos,
            IRepository<Package,string> packagesRepos,
            IRepository<PackageSort,string> packageSortsRepos,
            IDeliverOutletAllocator deliverOutletAllocator,
            ISettingProvider settingProvider
            )
        {
            this.deliverProcessRepos = deliverProcessContextsRepository;
            this.loadrobotTaskRepos = opStationDeviceTasksRepository;
            this.hBAgvTasksRepos = hBAgvTasksRepos;
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.operationStationsRepos = operationStationsRepos;
            this.cageCarsRepos = cageCarsRepos;
            this.packagesRepos = packagesRepos;
            this.packageSortsRepos = packageSortsRepos;
            this.deliverOutletAllocator = deliverOutletAllocator;
            this.settingProvider = settingProvider;
        }

        public async Task HandleEventAsync(EntityStateChangingEvent<HBAgvTask, HBAgvTaskState> eventData)
        {
            //agv分配
            if (eventData.Entity.TaskName == HBAgvTask.AllocationAt)
            {
                switch (eventData.Entity.TaskState)
                {
                    case HBAgvTaskState.FinishedAndWaitting:

                        //HB agv分配成功，则：
                        var ProcessContext = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                        if (ProcessContext.DetailState == DeliverProcessState.WAITING_AGV)  //原来的状态是等待设备，否则是异常。
                        {
                            //1.添加“放置任务”
                            var Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                            var LoadRobotId = (await this.operationStationsRepos.GetAsync(Process.OperationStationId)).LoadRobotId;
                            var OpStationDeviceTask = new PackageLoadrobotTask(Guid.NewGuid().ToString(), LoadRobotId,Process.Id);
                            await loadrobotTaskRepos.InsertAsync(OpStationDeviceTask);

                            //2.更新流程所用的agv和流程的状态为“正在放置”
                            ProcessContext.ClaimAgv(eventData.Entity.AgvId);
                            ProcessContext.ClaimDetailState(DeliverProcessState.LOADING_PACKING);
                            await deliverProcessRepos.UpdateAsync(ProcessContext);
                        }
                        else
                        {
                            //error
                        }
                        break;

                    case HBAgvTaskState.CancelAndRelease:
                        //todo:分配失败，后续增加

                        break;

                    case HBAgvTaskState.NotStart:
                    case HBAgvTaskState.MovingToTarget:
                        //不需要处理
                        break;

                    case HBAgvTaskState.FinishedAndRelease:
                    case HBAgvTaskState.CancelAndWaitting:
                    default:
                        //不应该存在这些状态
                        break;
                }
            }

            //agv投递
            else if (eventData.Entity.TaskName == HBAgvTask.Deliver)
            {
                switch (eventData.Entity.TaskState)
                {
                    case HBAgvTaskState.FinishedAndRelease:

                        //agv投递成功，则：
                        var Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                        if (Process.DetailState == DeliverProcessState.AGV_DELIVERING)
                        {
                            //1.更新流程的状态为“投递成功”
                            Process.ClaimDetailState(DeliverProcessState.DELIVER_SUCCESS);
                            await this.deliverProcessRepos.UpdateAsync(Process);

                            //2.添加笼车对包裹的关联。
                            var cageCar = (await this.cageCarsRepos.WithDetailsAsync(p=>p.Packages)).Where(p => p.Id == Process.CageCarId).FirstOrDefault();
                            cageCar.AddPackageLink(Process.PackageId);
                            await this.cageCarsRepos.UpdateAsync(cageCar);

                            //3.清除投递口对工作流 关联。
                            var deliverOutlet = (await this.deliverOutletsRepos.WithDetailsAsync(p => p.HandlingProcess)).Where(p => p.Id == Process.DeliverOutletId).FirstOrDefault();
                            deliverOutlet.RemoveHandlingProcessLink(Process);
                            await this.deliverOutletsRepos.UpdateAsync(deliverOutlet);
                        }
                        else
                        {
                            //error
                        }
                        break;

                    case HBAgvTaskState.CancelAndRelease:

                        Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                        if (Process.DetailState == DeliverProcessState.AGV_DELIVERING)
                        {
                            //1.更新流程的状态为“投递失败”
                            Process.ClaimDetailState(DeliverProcessState.DELIVER_FAULT);
                            await this.deliverProcessRepos.UpdateAsync(Process);

                            //2.清除投递口对工作流 关联。
                            //var deliverOutlet = await this.deliverOutletsRepos.GetAsync(Process.DeliverOutletId);
                            var deliverOutlet = (await this.deliverOutletsRepos.WithDetailsAsync(p => p.HandlingProcess)).Where(p => p.Id == Process.DeliverOutletId).FirstOrDefault();
                            deliverOutlet.RemoveHandlingProcessLink(Process);
                            await this.deliverOutletsRepos.UpdateAsync(deliverOutlet);
                        }
                        else
                        {
                            //error
                        }

                        break;

                    case HBAgvTaskState.NotStart:
                    case HBAgvTaskState.MovingToTarget:
                        //不需要处理
                        break;

                    case HBAgvTaskState.FinishedAndWaitting:
                    case HBAgvTaskState.CancelAndWaitting:
                    default:
                        //不应该存在这些状态
                        break;
                }
            }

            //agv行走
            else if (eventData.Entity.TaskName == HBAgvTask.Move)
            {
                switch (eventData.Entity.TaskState)
                {
                    case HBAgvTaskState.FinishedAndWaitting:
                        //1.确定投递口
                        var Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                        if (Process.DetailState == DeliverProcessState.IDLE_DRIVING)
                        {
                            var Package = await this.packagesRepos.GetAsync(Process.PackageId);
                            var PackageSort = await this.packageSortsRepos.GetAsync(Package.PackageSortId);
                            var Outlets = (await this.deliverOutletsRepos.WithDetailsAsync(p => p.HandlingProcess)).ToList();
                            var DeliverOutletForProcess = this.deliverOutletAllocator.AllocateDeliverOutlet(PackageSort, Outlets);
                            if (DeliverOutletForProcess != null)    //有适合投递口              
                            {
                                //投递口关联对应工作流
                                DeliverOutletForProcess.AddHandlingProcessLink(Process);
                                await this.deliverOutletsRepos.UpdateAsync(DeliverOutletForProcess);

                                //工作流关联对应投递口和更新状态
                                Process.AllocateDeliverOutlet(DeliverOutletForProcess);
                                Process.ClaimDetailState(DeliverProcessState.AGV_DELIVERING);
                                await this.deliverProcessRepos.UpdateAsync(Process);

                                //添加HB投递任务
                                var AgvAllocationTask = HBAgvTask.CreatDeliverTask(eventData.Entity.DeliverProcessTaskId, DeliverOutletForProcess.UppperMapNodeName, Guid.NewGuid().ToString(), Process.AgvId);
                                await this.hBAgvTasksRepos.InsertAsync(AgvAllocationTask);
                            }
                            else  //没有适合投递口
                            {
                                ////更新状态
                                //Process.ClaimDetailState(DeliverProcessState.IDLE_DRIVING);
                                //await this.deliverProcessRepos.UpdateAsync(Process);

                                //添加HB溜车任务
                                string MoveTaskNode =await GetMoveTargetName(eventData.Entity.TargetNodeName);
                                var AgvAllocationTask = HBAgvTask.CreatMoveTask(eventData.Entity.DeliverProcessTaskId, MoveTaskNode, Guid.NewGuid().ToString(), Process.AgvId);
                                await this.hBAgvTasksRepos.InsertAsync(AgvAllocationTask);
                            }
                        }
                        else
                        {
                            //error

                        }
                        break;

                    case HBAgvTaskState.CancelAndRelease:

                        Process = await this.deliverProcessRepos.GetAsync(eventData.Entity.DeliverProcessTaskId);
                        if (Process.DetailState == DeliverProcessState.IDLE_DRIVING)
                        {
                            //1.更新流程的状态为“投递失败”
                            Process.ClaimDetailState(DeliverProcessState.IDLE_DRIVING_FAULT);
                            await this.deliverProcessRepos.UpdateAsync(Process);

                            //2.清除投递口对工作流 关联。
                            //var deliverOutlet = await this.deliverOutletsRepos.GetAsync(Process.DeliverOutletId);
                            var deliverOutlet = (await this.deliverOutletsRepos.WithDetailsAsync(p => p.HandlingProcess)).Where(p => p.Id == Process.DeliverOutletId).FirstOrDefault();
                            deliverOutlet.RemoveHandlingProcessLink(Process);
                            await this.deliverOutletsRepos.UpdateAsync(deliverOutlet);
                        }
                        else
                        {
                            //error
                        }

                        break;

                    case HBAgvTaskState.NotStart:
                    case HBAgvTaskState.MovingToTarget:
                        //不需要处理
                        break;

                    case HBAgvTaskState.FinishedAndRelease:
                    case HBAgvTaskState.CancelAndWaitting:
                    default:
                        //不应该存在这些状态
                        break;
                }

                
            }

            else
            {
                throw new Exception("不存在的任务");
            }


            var abc = deliverOutletsRepos.GetDbContext().Set<DeliverOutlet>().Local;
        }

        private async Task<string> GetMoveTargetName(string LastTargetName)
        {
            //从配置中获取节点
            var TargetsStr = await this.settingProvider.GetOrNullAsync(PDSSettingDefinitionProvider.AgvSlideNodeNames);
            var tempTargets = TargetsStr.Split(new char[] { ',','，'});
            List<string> Targets = new List<string>();
            foreach(var t in tempTargets)
            {
                Targets.Add(t.Trim());
            }


            //确定节点
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
