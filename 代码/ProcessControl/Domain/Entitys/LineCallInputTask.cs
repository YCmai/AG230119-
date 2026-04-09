using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using HBTaskModule.Domain;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS.LineCallProcessTaskModule.Domain
{

    public class LineCallInputTask : TaskBaseAggregateRoot
    {
        protected LineCallInputTask()
        {
            this.State = LineCallInpputTaskState.NotStart;
        }

        public LineCallInputTask(Guid id,string loadNodeName,  int loadHeight, string unloadNodeName, int unloadHeight, string wareHouseId) : base(id, null, "")
        {
            this.Id = id;
            this.State = LineCallInpputTaskState.NotStart;
            this.LoadNodeName = loadNodeName;
            this.LoadHeight = loadHeight;
            this.UnloadNodeName = unloadNodeName;
            this.UnloadHeight = unloadHeight;
            this.WareHouseId = wareHouseId;
        }

        public LineCallInpputTaskState State { get; private set; }
        /// <summary>
        /// 分配的处理这个流程的设备Id,系统成功分配后有此数据。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string DeviceId { get; internal set; }
        /// <summary>
        /// 取货节点
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string LoadNodeName { get; private set; }
        /// <summary>
        /// 取货高度
        /// </summary>
        public int LoadHeight { get; private set; }
        /// <summary>
        /// 卸货节点
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string UnloadNodeName { get; private set; }
        /// <summary>
        /// 卸货高度
        /// </summary>
        public int UnloadHeight { get; private set; }
        /// <summary>
        /// 仓库Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }

        public void SetStatus(LineCallInpputTaskState NewState)
        {
            if (this.State != NewState)
            {
                this.State = NewState;
                if (!this.CloseTime.HasValue &&
                    (this.State == LineCallInpputTaskState.Cancel || this.State == LineCallInpputTaskState.FinishedAndClosed))
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }
    }


    public enum LineCallInpputTaskState : int
    {
        /// <summary>
        /// 未开始（过渡态）
        /// </summary>
        NotStart = 0,
        /// <summary>
        /// 正在分配agv。（过渡态）
        /// </summary>
        AllocatingAgv = 1,
        /// <summary>
        /// 正在取货（过渡态）
        /// </summary>
        AgvLoading = 2,
        /// <summary>
        /// 取货异常，无法继续取货（过渡态）
        /// </summary>
        AgvLoadFalse = 3,
        /// <summary>
        /// 正在前往卸货（过渡态）
        /// </summary>
        AgvUnloading = 4,
        /// <summary>
        /// 卸货异常，无法正常卸货（过渡态）
        /// </summary>
        AgvUnloadFalse = 5,
        /// <summary>
        /// 已经完成，agv已经释放(最终态）
        /// </summary>
        FinishedAndClosed = 7,
        /// <summary>
        /// 取消，物料在原来的位置上，agv已经释放（最终态）
        /// </summary>
        Cancel = 8,
    }


}
