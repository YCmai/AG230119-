using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;

namespace HBTaskModule.Domain
{
    /// <summary>
    /// 心跳取货任务。
    /// </summary>
    public class HBTask_Release : TaskBaseAggregateRoot
    {
        protected HBTask_Release() { }

        public HBTask_Release(Guid Id, Guid fatherTaskId, string fatherTaskType) : base(Id, fatherTaskId, fatherTaskType)
        {
            this.ReleaseStatus = EReleaseStatus.NotSync;
        }

        public  EReleaseStatus ReleaseStatus { get; private set; }

        public void SetStatus(EReleaseStatus status)
        {
            if (this.ReleaseStatus != status)
            {
                this.ReleaseStatus = status;

                if (!this.CloseTime.HasValue && this.ReleaseStatus == EReleaseStatus.Success)
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }


    }



    public enum EReleaseStatus
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
        /// 已完成：物料已经正确卸到制定位置（结束态）
        /// </summary>
        Success
    }

}
