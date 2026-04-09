using System;
using System.ComponentModel.DataAnnotations.Schema;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;

namespace HBTaskModule.Domain
{
    /// <summary>
    /// 心跳分配请求任务。
    /// </summary>
    public class HBTask_Move : TaskBaseAggregateRoot
    {
        protected HBTask_Move() { }

        public HBTask_Move(Guid Id, Guid fatherTaskId, string fatherTaskType, string targetPosition) : base(Id, fatherTaskId, fatherTaskType)
        {
            this.TargetPosition = targetPosition;

            this.MoveStatus = EMoveStatus.NotSync;
        }

        /// <summary>
        /// 表示目标位置所在的地图节点名称
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string TargetPosition { get; private set; }

        public EMoveStatus MoveStatus { get; private set; }



        internal void SetStatus(EMoveStatus status)
        {
            if (this.MoveStatus != status)
            {
                this.MoveStatus = status;

                if (!this.CloseTime.HasValue &&
                    (this.MoveStatus == EMoveStatus.Cancel ||
                    this.MoveStatus == EMoveStatus.Fault ||
                    this.MoveStatus == EMoveStatus.Success)
                    )
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }

    }


    public enum EMoveStatus
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
        /// 执行中或者已安排执行
        /// </summary>
        Excuting,
        /// <summary>
        /// 移动失败，任务无法继续
        /// </summary>
        Fault,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel,
        /// <summary>
        /// 移动成功
        /// </summary>
        Success
    }
}

