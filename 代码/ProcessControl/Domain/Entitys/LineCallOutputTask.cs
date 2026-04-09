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
    public class LineCallOutputTask : TaskBaseAggregateRoot
    {
        protected LineCallOutputTask()
        {
            this.State = LineCallOutputTaskState.NotStart;
        }

        public LineCallOutputTask(Guid id, string pickNodeName, string unloadNodeName) : base(id, null, "")
        {
            this.State = LineCallOutputTaskState.NotStart;
            this.PickNodeName = pickNodeName;
            this.UnloadNodeName = unloadNodeName;
        }
        /// <summary>
        /// 执行方式
        /// </summary>
        public CallLineOutputTaskExcuteType ProcessExcuteType { get; private set; }

        public LineCallOutputTaskState State { get; private set; }
        /// <summary>
        /// 分配的处理这个流程的设备Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string DeviceId { get;  set; }
        /// <summary>
        /// 是否需要确认拣货
        /// </summary>
        public bool NeedConfirmPick { get;private set; }
        /// <summary>
        /// 取货节点名称
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string PickNodeName { get; private set; }
        /// <summary>
        /// 取货高度
        /// </summary>
        public int PickHeight { get; private set; }
        /// <summary>
        /// 卸货节点名称
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


        public void SetStatus(LineCallOutputTaskState NewState)
        {
            if (this.State != NewState)
            {
                this.State = NewState;
                if (!this.CloseTime.HasValue &&
                    (this.State == LineCallOutputTaskState.Cancel || this.State == LineCallOutputTaskState.FinishedAndClosed))
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }

    }



    public enum LineCallOutputTaskState:int
    {
        /// <summary>
        /// 未启动（过渡态）
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
        /// 已经完成卸货，AGV释放，等待拣货确认。（过度态）
        /// </summary>
        UnloadedAndWaitPick = 6,
        /// <summary>
        /// 已经完成，agv已释放(最终态）
        /// </summary>
        FinishedAndClosed = 7,
        /// <summary>
        /// 取消，物料在原来储位上，agv已释放（最终态）
        /// </summary>
        Cancel = 8,
    }

    public enum CallLineOutputTaskExcuteType
    {
        /// <summary>
        /// 独立的任务
        /// </summary>
        Solo,
        /// <summary>
        /// 和呼叫入库合并
        /// </summary>
        CombineToCallLineInputProcess,

    }

}
