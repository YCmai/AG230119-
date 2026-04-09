using PDS.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    /// <summary>
    /// 投递流程上下文。
    /// </summary>
    public class DeliverProcessTask: BasicAggregateRoot<string>
    {
        protected DeliverProcessTask() { }

        public DeliverProcessTask(string Id, string PackageID, string OperationStationId)
        {
            this.Id = Id;
            this.PackageId = PackageID;
            this.OperationStationId = OperationStationId;
            this.CreationTime = DateTime.Now;
            this.DetailState = DeliverProcessState.WAITING_AGV;
            this.DeliverOutletId = "";
            this.DeliverOutletType =  DeliverOutletType.ABNORMAL;
            this.CageCarId = "";
        }

        /// <summary>
        /// 操作点
        /// </summary>
        public string OperationStationId { get; protected set; }
        /// <summary>
        /// 目标投递口
        /// </summary>
        public string DeliverOutletId { get; protected set; }  
        /// <summary>
        /// 投递口类型
        /// </summary>
        public DeliverOutletType DeliverOutletType { get; protected set; } 
        /// <summary>
        /// 目标笼车
        /// </summary>
        public string CageCarId { get; protected set; } 
        /// <summary>
        /// 包裹
        /// </summary>
        public string PackageId { get; protected set; } 
        /// <summary>
        /// 执行投递流程的Agv
        /// </summary>
        public string AgvId { get; protected set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreationTime { get; protected set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? CloseTime { get; protected set; }
        /// <summary>
        /// 投递流程的状态
        /// </summary>
        public DeliverProcessState DetailState { get; protected set; } = DeliverProcessState.WAITING_AGV;
        /// <summary>
        /// 包裹放置时间
        /// </summary>
        public DateTime? PackageLoadTime { get; protected set; }

        /// <summary>
        /// 指定使用的Agv
        /// </summary>
        /// <param name="AgvId"></param>
        public void ClaimAgv(string AgvId)
        {
            this.AgvId = AgvId;
        }


        public void ClaimDetailState(DeliverProcessState NewState)
        {
            if (this.DetailState != NewState)
            {
                var oldState = this.DetailState;
                this.DetailState = NewState;

                switch(this.DetailState)
                {
                    case DeliverProcessState.AGV_DELIVERING:
                        this.PackageLoadTime = DateTime.Now;
                        break;

                    case DeliverProcessState.IDLE_DRIVING:
                        this.PackageLoadTime = DateTime.Now;
                        break;

                    case DeliverProcessState.DELIVER_FAULT:
                    case DeliverProcessState.DELIVER_SUCCESS:
                    case DeliverProcessState.IDLE_DRIVING_FAULT:
                        this.CloseTime = DateTime.Now;
                        break;
                    default:
                        //不用处理
                        break;
                }

                AddLocalEvent(new EntityStateChangingEvent<DeliverProcessTask, DeliverProcessState>(this, oldState, NewState));
            }
        }

        public void AllocateDeliverOutlet(DeliverOutlet deliverOutlet)
        {
            if (!this.DeliverOutletId.IsNullOrEmpty()) throw new Exception("已经确定了投递口，无法修改投递口");  //主要是修改投递口的逻辑还没有做，包括相关的关联修改。
            //if (deliverOutlet.State != DeliverOutletState.DELIVER_AVAILABLE) throw new Exception("投递口状态不可用，无法分配投递口");
            this.DeliverOutletId = deliverOutlet.Id;
            this.DeliverOutletType = deliverOutlet.DeliverType;
            this.CageCarId = deliverOutlet.CageCarId;
        }
    }



    public enum DeliverProcessState : int
    {
        /// <summary>
        /// 初始状态。等待分配可用的AGV。
        /// </summary>
        WAITING_AGV = 0,
        /// <summary>
        /// 设备已经准备好，正在控制设备放到AGV上。
        /// </summary>
        LOADING_PACKING,
        /// <summary>
        /// 溜车中，暂时没有确定的投递口，为了不挡着后面的车，AGV带着包裹在随意跑。
        /// </summary>
        IDLE_DRIVING,
        /// <summary>
        /// 溜车异常，AGV溜车时出现异常，无法继续执行后续任务。效果跟投递异常一样。
        /// </summary>
        IDLE_DRIVING_FAULT,
        /// <summary>
        /// 包裹已经放到设备上，正在控制AGV投递。
        /// </summary>
        AGV_DELIVERING,
        /// <summary>
        /// 投递成功；包裹已经运到目标投递口。
        /// </summary>
        DELIVER_SUCCESS,
        /// <summary>
        /// 投递异常。投递是出现异常（或被取消agv的投递任务），投递行为已经终止，对应AGV不能继续执行任务。
        /// </summary>
        DELIVER_FAULT,
    }




}
