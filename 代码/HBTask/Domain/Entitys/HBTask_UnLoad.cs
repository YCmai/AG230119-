using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBTaskModule.Domain
{
    /// <summary>
    /// 心跳取货任务。
    /// </summary>
    public class HBTask_UnLoad : TaskBaseAggregateRoot
    {
        protected HBTask_UnLoad() { }

        public HBTask_UnLoad(Guid Id, Guid fatherTaskId, string fatherTaskType, string targetPosition) : base(Id, fatherTaskId, fatherTaskType)
        {
            this.TargetPosition = targetPosition;

            this.UnloadStatus = EUnloadStatus.NotSync;
        }
        /// <summary>
        /// 目标取货点（货架所在位置）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string TargetPosition { get; private set; }
        public EUnloadStatus UnloadStatus { get; private set; }


        public void SetStatus(EUnloadStatus status)
        {
            if (this.UnloadStatus != status)
            {
                this.UnloadStatus = status;

                if (!this.CloseTime.HasValue &&
                    (this.UnloadStatus == EUnloadStatus.Cancel ||
                    this.UnloadStatus == EUnloadStatus.Fault ||
                    this.UnloadStatus == EUnloadStatus.Success)
                    )
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }
    }

    public enum EUnloadStatus
    {
        /// <summary>
        /// 未同步
        /// </summary>
        NotSync = 0,
        /// <summary>
        /// 同步中
        /// </summary>
        Syncing = 1,
        /// <summary>
        /// 执行中或者已安排执行
        /// </summary>
        Excuting = 2,
        /// <summary>
        /// 未卸货，agv没有物料（结束态）
        /// </summary>
        Fault = 3,
        /// <summary>
        /// 取消，未卸货，任务取消（结束态）
        /// </summary>
        Cancel = 4,
        /// <summary>
        /// 已完成：物料已经正确卸到制定位置（结束态）
        /// </summary>
        Success = 5
    }
}

