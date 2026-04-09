using AciModule.Domain.Shared;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;

namespace AciModule.Domain.Entitys
{
    public class NdcTask_Moves : TaskBaseAggregateRoot
    {
        protected NdcTask_Moves()
        {

        }
        public NdcTask_Moves(Guid Id, Guid fatherTaskId, string fatherTaskType,int ndcTaskId,string schedulTaskNo, int taskType,string group, int pickupSite,int pickupHeight, int unloadSite,int unloadHeight, int priority):base(Id, fatherTaskId, fatherTaskType)
        {
            NdcTaskId = ndcTaskId;
            SchedulTaskNo = schedulTaskNo;
            TaskType = taskType;
            Group = group;
          
            PickupSite = pickupSite;
            PickupHeight = pickupHeight;
            UnloadSite = unloadSite;
            UnloadHeight = unloadHeight;
            Priority = priority;
            TaskStatus = TaskStatuEnum.None;
            CreationTime = DateTime.Now;
        }
        public void SetStatus(TaskStatuEnum taskStatu,int parameter=0)
        {
            this.TaskStatus = taskStatu;
            switch (taskStatu)
            {
                case TaskStatuEnum.ConfirmCar:
                    SetAgvId(parameter);
                    break;
                case TaskStatuEnum.TaskFinish:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.Canceled:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.CanceledWashing:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.CanceledWashFinish:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.RedirectRequest:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.InvalidUp:
                    SetFinishTime();
                    SetRemark(parameter);
                    break;
                case TaskStatuEnum.InvalidDown:
                    SetFinishTime();
                    SetRemark(parameter);
                    break;
                case TaskStatuEnum.OrderAgv:
                    SetFinishTime();
                    break;
                case TaskStatuEnum.OrderAgvFinish:
                    SetFinishTime();
                    break;
            }
        }
        public void SetNdcId(int id)
        {
            this.NdcTaskId = id;
        }
        public void SetAgvId(int agvId)
        {
            this.AgvId = agvId;
        }

        public void SetOrderIndex(int orderIndex)
        {
            this.OrderIndex = orderIndex;
        }
        public void SetRemark(int site)
        {
            this.Remark = string.Format("存在无效站点:{0}", site);
        }
        public void RecoveryId()
        {
            this.NdcTaskId = -1;
        }
        public void SetFinishTime()
        {
            this.CloseTime = DateTime.Now;
        }
        public void SetAgvBlank(int unloadSite,int unloadHeight)
        {
            this.UnloadSite = unloadSite;
            this.UnloadHeight = unloadHeight;
        }
        /// <summary>
        /// 当前值用于mes返回的能卸货或直接返回原点的状态
        /// </summary>
        /// <param name="depth"></param>
        public void SetUnloadDepth(int depth)
        {
            this.UnloadDepth = depth;
        }
        /// <summary>
        /// 下发给ndc 调度的任务ID
        /// </summary>
        public virtual int NdcTaskId { get; protected set; }
        /// <summary>
        /// 上位下发调度任务编号
        /// </summary>
        public virtual string? SchedulTaskNo { get; protected set; }
        /// <summary>
        /// 任务类型成品入库第一段：1，空桶出库到产线：2
        /// </summary>
        public virtual int TaskType { get; protected set; }
        /// <summary>
        /// 任务分组
        /// </summary>
        public virtual string Group{ get; set; }
       
      
        /// <summary>
        /// 取货站点
        /// </summary>
        public virtual int PickupSite { get; protected set; }
        /// <summary>
        /// 取货高度
        /// </summary>
        public virtual int PickupHeight { get; protected set; }
        /// <summary>
        /// 取货深度
        /// </summary>
        public virtual int PickUpDepth { get; protected set; } = 0;
        /// <summary>
        /// 卸货站点
        /// </summary>
        public virtual int UnloadSite { get; protected set; }
        /// <summary>
        /// 卸货高度
        /// </summary>
        public virtual int UnloadHeight { get; protected set; }
        /// <summary>
        /// 卸货深度
        /// </summary>
        public virtual int UnloadDepth { get; protected set; } = 0;
        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual TaskStatuEnum TaskStatus { get; protected set; }
        /// <summary>
        /// 执行agv
        /// </summary>
        public virtual int AgvId { get; protected set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int Priority { get; protected set; }
        /// <summary>
        /// 任务备注
        /// </summary>
        public virtual string? Remark { get; protected set; }

        /// <summary>
        /// 取消任务
        /// </summary>
        public virtual bool CancelTask { get; set; } = false;


        /// <summary>
        /// 订单编号
        /// </summary>
        public virtual int OrderIndex { get; protected set; } = 0;

    }
}
