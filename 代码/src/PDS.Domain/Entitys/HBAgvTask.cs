using PDS.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    public class HBAgvTask : BasicAggregateRoot<string>
    {
        public const string Deliver = "Deliver";            //控制agv执行投递,释放agv。  结束状态有：完成并释放，取消并释放。
        public const string Move = "MoveWithLoad";  //控制agv带着包裹行走，不释放agv。  结束状态有：完成并等待，取消并释放。
        public const string AllocationAt = "AllocationAt";  //在目标位置分配一台agv；不释放agv。  结束状态有：完成并等待，取消并释放。

        protected HBAgvTask() { }

        #region 本项目中用到的3种设备任务

        public static HBAgvTask CreatMoveTask(string packageDeliverProcessTaskId, string TargetNodeName, string taskIdInHB,string TargetAgvId)
        {
            HBAgvTask hBDeviceTask = new HBAgvTask();
            hBDeviceTask.Id = Guid.NewGuid().ToString();
            hBDeviceTask.DeliverProcessTaskId = packageDeliverProcessTaskId;
            hBDeviceTask.TaskName = HBAgvTask.Move;
            hBDeviceTask.TargetNodeName = TargetNodeName;
            hBDeviceTask.NeedReleaseAgv = false; //不释放agv
            hBDeviceTask.TaskIdInHB = taskIdInHB;
            hBDeviceTask.CreationTime = DateTime.Now;
            hBDeviceTask.TargetAgvId = TargetAgvId;
            return hBDeviceTask;
        }

        public static HBAgvTask CreatAllocationTask(string packageDeliverProcessTaskId, string OperationStationNodeName, string taskIdInHB)
        {
            HBAgvTask hBDeviceTask = new HBAgvTask();
            hBDeviceTask.Id = Guid.NewGuid().ToString();
            hBDeviceTask.DeliverProcessTaskId = packageDeliverProcessTaskId;
            hBDeviceTask.TaskName = HBAgvTask.AllocationAt;
            hBDeviceTask.TargetNodeName = OperationStationNodeName;
            hBDeviceTask.NeedReleaseAgv = false; //不释放agv
            hBDeviceTask.TaskIdInHB = taskIdInHB;
            hBDeviceTask.CreationTime = DateTime.Now;
            return hBDeviceTask;
        }

        public static HBAgvTask CreatDeliverTask(string packageDeliverProcessTaskId, string DeliverNodeName,string taskIdInHB,string TargetAgvId)
        {
            HBAgvTask hBDeviceTask = new HBAgvTask();
            hBDeviceTask.Id = Guid.NewGuid().ToString();
            hBDeviceTask.DeliverProcessTaskId = packageDeliverProcessTaskId;
            hBDeviceTask.TaskName = HBAgvTask.Deliver;
            hBDeviceTask.TargetNodeName = DeliverNodeName;
            hBDeviceTask.NeedReleaseAgv = true; //释放agv。
            hBDeviceTask.TaskIdInHB = taskIdInHB;
            hBDeviceTask.CreationTime = DateTime.Now;
            hBDeviceTask.TargetAgvId = TargetAgvId;
            return hBDeviceTask;
        }
        
        #endregion
        /// <summary>
        /// 流程上下文
        /// </summary>
        public string DeliverProcessTaskId { get; protected set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public HBAgvTaskState TaskState { get; protected set; } = HBAgvTaskState.NotStart;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TargetNodeName { get; protected set; }
        /// <summary>
        /// 当前在使用的Agv
        /// </summary>
        public string AgvId { get; protected set; }
        /// <summary>
        /// 需要指定使用的agvId
        /// </summary>
        public string TargetAgvId { get; protected set; }
        /// <summary>
        /// 心跳系统中的TaskId
        /// </summary>
        public string TaskIdInHB { get; protected set; }
        /// <summary>
        /// 是否需要释放agv
        /// </summary>
        public bool NeedReleaseAgv { get; protected set; }
        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreationTime { get; private set; }

        public DateTime CloseTime { get; private set; }

        public void ClaimTaskState(HBAgvTaskState NewState)
        {
            if (this.TaskState != NewState)
            {
                var oldState = this.TaskState;
                this.TaskState = NewState;

                switch(NewState)
                {
                    case HBAgvTaskState.CancelAndRelease:
                    case HBAgvTaskState.CancelAndWaitting:
                    case HBAgvTaskState.FinishedAndRelease:
                    case HBAgvTaskState.FinishedAndWaitting:
                        this.CloseTime = DateTime.Now;
                        break;

                    default:
                        break;
                }

                AddLocalEvent(new EntityStateChangingEvent<HBAgvTask, HBAgvTaskState>(this, oldState, NewState));
            }
        }

        public void ClaimAgv(string AgvId)
        {
            this.AgvId = AgvId;
        }
    }


    public enum HBAgvTaskState
    {
        /// <summary>
        /// 当前认为还没有开始
        /// </summary>
        NotStart,
        /// <summary>
        /// 当前任务正在前往目标点
        /// </summary>
        MovingToTarget,
        /// <summary>
        ///  当前任务已经完成，agv已释放。
        /// </summary>
        FinishedAndRelease,
        /// <summary>
        /// 当前任务没有执行完成，已经取消，agv已释放。
        /// </summary>
        CancelAndRelease,
        /// <summary>
        /// 当前任务已完成，agv没有被释放
        /// </summary>
        FinishedAndWaitting,
        /// <summary>
        /// 当前任务没有执行完成，已取消，agv没有被释放
        /// </summary>
        CancelAndWaitting,
    }




    
}
