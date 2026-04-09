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
    public class HBTask_Load : TaskBaseAggregateRoot
    {

        protected HBTask_Load() { }

        public HBTask_Load(Guid Id, Guid fatherTaskId, string fatherTaskType, string targetPosition, EMaterialCheckType materialCheckType,string checkMaterialCode) : base(Id, fatherTaskId, fatherTaskType)
        {
            this.TargetPosition = targetPosition;
            this.MaterialCheckType = materialCheckType;
            this.CheckMaterialCode = checkMaterialCode;

            this.LoadStatus = ELoadStatus.NotSync;
        }
        /// <summary>
        /// 目标取货点（货架所在位置）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string TargetPosition { get; private set; }
        /// <summary>
        /// 对货物编码检查类型
        /// 0: 不检查
        /// 1：取货前匹配物料编码，匹配失败则不取货。
        /// 2. 取货时检查物料编码，匹配失败则不取货。
        /// 3. 取货后检查物料编码。
        /// 注意：如果需要检查物料，需要确定项目使用到的agv是否支持并配置了相关的功能。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public EMaterialCheckType MaterialCheckType { get;private set; }
        /// <summary>
        /// 货物编码。
        /// 为空时，直接进行搬运；
        /// 不为空时，在agv举升料架前，会进行料架编号匹配，如果不一致，则认为异常，不会进行取货。
        /// 注意：对于不能识别货物信息的agv，此参数必须为空。使用此功能时，还需要确保agv的货物识别功能的编码格式是否支持。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string CheckMaterialCode { get; private set; }
        /// <summary>
        /// 实际物料编码（空表示没有或者未知）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string ActualMaterialCode { get; internal set; }

        public ELoadStatus LoadStatus { get; private set; }



        public void SetStatus(ELoadStatus status)
        {
            if (this.LoadStatus != status)
            {
                this.LoadStatus = status;

                if (!this.CloseTime.HasValue &&
                    (this.LoadStatus == ELoadStatus.Cancel ||
                    this.LoadStatus == ELoadStatus.Fault ||
                    this.LoadStatus == ELoadStatus.Success)
                    )
                {
                    this.CloseTime = DateTime.Now;
                }
            }

        }

    }

    public enum EMaterialCheckType
    {
        /// <summary>
        /// 不检查
        /// </summary>
        NeednotCheck = 0,
        /// <summary>
        /// 取货前匹配物料编码，匹配失败则不取货
        /// </summary>
        CheckBeforeLoad = 1,
        /// <summary>
        /// 取货时检查物料编码，匹配失败则不取货
        /// </summary>
        CheckWhenLoading = 2,
        /// <summary>
        /// 取货后检查物料编码
        /// </summary>
        CheckAfterLoad = 3
    }

    public enum ELoadStatus
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
        /// 执行中
        /// </summary>
        Excuting,
        /// <summary>
        /// 执行失败，任务无法继续,未取货，物料不匹配（结束态）
        /// </summary>
        Fault,
        /// <summary>
        /// 未取货，任务取消（结束态）
        /// </summary>
        Cancel,
        /// <summary>
        /// 已完成：物料已经正确获取（结束态）
        /// </summary>
        Success,
        /// <summary>
        /// 已取货，但物料不匹配（结束态）
        /// </summary>
    }
}
