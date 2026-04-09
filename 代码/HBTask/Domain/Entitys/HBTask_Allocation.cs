using System;
using System.ComponentModel.DataAnnotations.Schema;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;

namespace HBTaskModule.Domain
{
    /// <summary>
    /// 心跳分配请求任务。
    /// </summary>
    public class HBTask_Allocation : TaskBaseAggregateRoot
    {
        protected HBTask_Allocation() { }

        public HBTask_Allocation(Guid Id, Guid fatherTaskId, string fatherTaskType, string targetAgvId, EAgvType agvType, EAllcateType allcateType, string targetPosition) 
            : base(Id, fatherTaskId, fatherTaskType)
        {
            this.TargetAgvId = targetAgvId;
            this.AgvType = agvType;
            this.AllcateType = allcateType;
            this.TargetPosition = targetPosition;

            this.AllcateStatus = EAllocationStatus.NotSync;
        }

        /// <summary>
        /// 用来指定需要分配的agvId，为空表示任意的agv都可以。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string TargetAgvId { get; private set; }
        [Column(TypeName = "nvarchar(64)")]
        public string ActualAgvId { get; internal set; }
        /// <summary>
        /// 已经分配的设备id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string AgvIdAllcated { get; private set; }
        /// <summary>
        /// 此参数只有在agvId为空时有效。
        /// agv有8种类型，此参数用来告诉系统需要分配的agv类型。       
        /// 可以叠加类型，例如0xFF表示所有8种agv都可以。
        /// </summary>
        public EAgvType AgvType { get; private set; }
        /// <summary>
        /// 分配类型
        /// </summary>
        public EAllcateType AllcateType { get; private set; }
        /// <summary>
        /// 表示目标位置所在的地图节点名称。分配时，会优先安排跟此目标点更近的agv。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string TargetPosition { get; private set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public EAllocationStatus AllcateStatus { get; private set; } 

        public void SetStatus(EAllocationStatus status)
        {
            if(this.AllcateStatus!=status)
            {
                this.AllcateStatus = status;

                if (!this.CloseTime.HasValue &&
                    (this.AllcateStatus == EAllocationStatus.Cancel ||
                    this.AllcateStatus == EAllocationStatus.Fault ||
                    this.AllcateStatus == EAllocationStatus.Success)
                    )
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }


    }


    /// <summary>
    /// 设备类型（每一种类型对应不同的轮廓和驱动类型）,一个地图最多设八种类型;每个类型一个位
    /// </summary>
    public enum EAgvType : int
    {
        Type1 = 0x01 << 0,
        Type2 = 0x01 << 1,
        Type3 = 0x01 << 2,
        Type4 = 0x01 << 3,
        Type5 = 0x01 << 4,
        Type6 = 0x01 << 5,
        Type7 = 0x01 << 6,
        Type8 = 0x01 << 7,
        AnyType = 0xFF
    }


    public enum EAllcateType
    {
        /// <summary>
        /// agv到达目标点时分配。
        /// </summary>
        AllocateWhenReach = 0,
        /// <summary>
        /// agvCatch目标点时分配
        /// </summary>
        AllocateWhenCatch = 1,
        /// <summary>
        /// 不需要跑到指定位置，直接分配。
        /// </summary>
        AllocateAtLocalPosition = 2,
    }


    public enum EAllocationStatus
    {
        /// <summary>
        /// 未同步
        /// </summary>
        NotSync,
        /// <summary>
        /// 同步中
        /// </summary>
        Syncing,
        /// <summary>
        /// 已经同步，等待执行完成
        /// </summary>
        Excuting,
        /// <summary>
        /// 分配失败（最终态）
        /// </summary>
        Fault,
        /// <summary>
        /// 分配被取消。没有分配到agv。（最终态）
        /// </summary>
        Cancel,
        /// <summary>
        /// 分配完成（最终态）。
        /// </summary>
        Success
    }
}

