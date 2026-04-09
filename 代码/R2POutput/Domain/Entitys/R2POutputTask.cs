using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using HBTaskModule.Domain;
using TaskBaseModule.Domain;
using TaskBaseModule.Domain.Shared;

namespace WMS.R2POutputModule.Domain
{
    /// <summary>
    /// 这是一个动态经过任务点的任务，项目先在指定取货位置取货，接着按顺序经过一个一个路过点，最后在指定卸货位置进行卸货。
    /// </summary>
    public class R2POutputTask : TaskBaseAggregateRoot
    {
        protected R2POutputTask()
        {
            this.State = R2POutputTaskState.NotStart;
        }

        public R2POutputTask(Guid id,string loadNodeName,string unloadNodeName)
        {
            this.Id = id;
            this.State = R2POutputTaskState.NotStart;
            this.LoadNodeName = loadNodeName;
            this.UnloadNodeName = unloadNodeName;
        }

        public R2POutputTaskState State { get; private set; }
        /// <summary>
        /// 分配的处理这个流程的设备Id,系统成功分配后有此数据。
        /// </summary>
        public string DeviceId { get; internal set; }
        /// <summary>
        /// 取货节点
        /// </summary>
        public string LoadNodeName { get; private set; }
        /// <summary>
        /// 卸货节点
        /// </summary>
        public string UnloadNodeName { get; private set; }
        /// <summary>
        /// 节点，如果有多个，用逗号分开。
        /// </summary>
        public string PassNodemName { get; private set; }
        /// <summary>
        /// 当前的目标节点。（如果状态为AgvDelivering，表示正在前往这个点。如果状态为AgvDeliverFinished，表示已经到达了这个点）。其余状态下，这个点是空的。
        /// </summary>
        public string CurrentPassNodeName { get; private set; }

        public List<string> GetPassNodeNames()
        {
            return PassNodemName.Split(',', '，').ToList();
        }


        
        internal void AddPassNodeName(string NewPassNodeName)
        {
            //目前规定只有这些状态可以添加路过点。
            if (State == R2POutputTaskState.NotStart
                || State == R2POutputTaskState.AllocatingAgv
                || State == R2POutputTaskState.AgvLoading
                || State == R2POutputTaskState.AgvDelivering
                || State == R2POutputTaskState.AgvDeliverFinished)

                PassNodemName += "," + NewPassNodeName;

            else throw new Exception("当前状态下无法添加新的路过节点" + this.State);
        }

        public void SetStatus(R2POutputTaskState NewState)
        {
            if (this.State != NewState)
            {
                this.State = NewState;
                if (!this.CloseTime.HasValue &&
                    (this.State == R2POutputTaskState.Cancel || this.State == R2POutputTaskState.FinishedAndClosed))
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }
    }


    public enum R2POutputTaskState : int
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
        /// 正在送货（过渡态）（如果需要送多个地方，或多次出现这个状态）
        /// </summary>
        AgvDelivering = 4,
        /// <summary>
        /// 送货成功，当前在送货节点。（过渡态）（如果需要送多个地方，或多次出现这个状态）
        /// </summary>
        AgvDeliverFinished = 5,
        /// <summary>
        /// 前往经过点的过程中出现异常，且无法继续任务。（过渡态）
        /// </summary>
        AgvDeliverFault = 6,
        /// <summary>
        /// 正在前往卸货（过渡态）
        /// </summary>
        AgvUnloading = 7,
        /// <summary>
        /// 卸货异常，无法正常卸货（过渡态）
        /// </summary>
        AgvUnloadFalse = 8,
        /// <summary>
        /// 已经完成，agv已经释放(最终态）
        /// </summary>
        FinishedAndClosed = 9,
        /// <summary>
        /// 取消，agv已经释放。可能是部分站点已经完成运送工作，也可能是完全没有完成。（最终态）
        /// </summary>
        Cancel = 10,
    }


    public class PassNodeInfo
    {
        /// <summary>
        /// 到达时间，如果有数据，表示已经到达了。
        /// </summary>
        public DateTime? ReachTime { get; private set; }
    }

}
