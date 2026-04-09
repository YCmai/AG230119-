using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    public class DeliverOutlet : BasicAggregateRoot<string>
    {
        protected DeliverOutlet() { }

        public DeliverOutlet(string Id, DeliverOutletType deliverOutletType, string uppperMapNodeName, string downMapMapNodeName, string packageSortId)
        {
            this.Id = Id;
            this.State = DeliverOutletState.FREE;
            this.DeliverType = deliverOutletType;
            this.UppperMapNodeName = uppperMapNodeName;
            this.DownMapMapNodeName = downMapMapNodeName;
            this.PackageSortId = packageSortId;
        }
        /// <summary>
        /// 包裹的分类，如果是空，则表示投递口还没有分类
        /// </summary>
        public string PackageSortId { get; protected set; }
        /// <summary>
        /// 停靠本投递口的笼车最大投递数量
        /// </summary>
        public int MaxPackageCount { get; protected set; }
        /// <summary>
        /// 投递口状态
        /// </summary>
        public DeliverOutletState State { get; protected set; }
        /// <summary>
        /// 投递口类型
        /// </summary>
        public DeliverOutletType DeliverType { get; protected set; }
        /// <summary>
        /// 绑定的上层地图节点（这个用于投递agv任务的控制）
        /// </summary>
        public string UppperMapNodeName { get; protected set; } = "";
        /// <summary>
        /// 绑定的下层地图节点（这个用于笼车的关联，如果笼车当前停靠点与这个点一致，则可以关联）
        /// </summary>
        public string DownMapMapNodeName { get; protected set; } = "";
        /// <summary>
        /// 笼车Id；有笼车Id，才能进行投递；
        /// </summary>
        public string CageCarId { get; protected set; } = "";
        /// <summary>
        /// 当前绑定的笼车的包裹数量统计
        /// </summary>
        public int CageCarPackageCount { get; protected set; }
        /// <summary>
        /// 未完成的关联的投递工作流。
        /// </summary>
        public virtual ICollection<DeliverOutletLinkDeliverProcess> HandlingProcess { get; protected set; } = new Collection<DeliverOutletLinkDeliverProcess>();
        /// <summary>
        /// 声明绑定.如果是空字符串,表示取消绑定
        /// </summary>
        /// <param name="carCarId"></param>
        /// <param name="CageCarPackageCount"></param>
        public void BindingCageCar(CageCar cageCar)
        {
            //只有空闲的时候才可以绑定
            if (this.State != DeliverOutletState.FREE) throw new Exception("当前投递口状态为" + this.State + "无法修改对笼车的绑定");
            this.CageCarId = cageCar.Id;
            this.CageCarPackageCount = cageCar.Packages.Count;
            this.State = DeliverOutletState.DELIVER_AVAILABLE;
            CheckState();
        }
        /// <summary>
        /// 取消绑定。
        /// </summary>
        public void ReleaseBingding(string cageCarId)
        {
            if (this.State == DeliverOutletState.FORBIDDEN || this.State == DeliverOutletState.FREE) throw new Exception("当前投递口状态为" + this.State + "无法取消对笼车的绑定");
            this.CageCarId = "";
            this.CageCarPackageCount = 0;
            this.State = DeliverOutletState.FREE;
        }



        /// <summary>
        /// 禁用投递口。只有投递口是空闲的时候才可以设置，否则会抛出异常。
        /// </summary>
        public void SetForbidden()
        {
            if (this.State != DeliverOutletState.FREE) throw new Exception("当前投递口状态为" + this.State + "无法禁止");
            this.State = DeliverOutletState.FORBIDDEN;
        }
        /// <summary>
        /// 取消对投递口的禁用。只有投递口是禁用的时候才能设置，否则会抛出异常。
        /// </summary>
        public void ResetForbidden()
        {
            if (this.State != DeliverOutletState.FORBIDDEN) throw new Exception("当前投递口状态为" + this.State + "无法取消禁止");
            this.State = DeliverOutletState.FREE;
        }

        public void ClaimPackageSortId(string packageSortId)
        {
            if (!this.CageCarId.IsNullOrWhiteSpace()) throw new Exception("投递口已经绑定了笼车,无法更换类型");

            this.PackageSortId = packageSortId;
        }

        public void ClaimMaxPackageCount(int maxPackageCount)
        {
            if (maxPackageCount <= 0) throw new Exception("投递口最大包裹数量不能小于0等于0");
            this.MaxPackageCount = maxPackageCount;
            CheckState();

        }

        public void AddHandlingProcessLink(DeliverProcessTask deliverProcess)
        {
            if (this.HandlingProcess.Any(p => p.DeliverProcessId == deliverProcess.Id)) throw new Exception("已经有了");

            if (deliverProcess.DetailState == DeliverProcessState.DELIVER_FAULT
                || deliverProcess.DetailState == DeliverProcessState.IDLE_DRIVING_FAULT
                || deliverProcess.DetailState == DeliverProcessState.DELIVER_SUCCESS)
            {
                throw new Exception("不允许关联添加已经完成的流程");
            }

            this.HandlingProcess.Add(new DeliverOutletLinkDeliverProcess(this.Id, deliverProcess.Id));
            CheckState();


        }

        /// <summary>
        /// 移除,如果移除的包裹是投递成功，则会进行统计，统计数量的更改会导致投递状态的变化。
        /// </summary>
        /// <param name="deliverProcess"></param>
        /// <returns></returns>
        public void RemoveHandlingProcessLink(DeliverProcessTask deliverProcess)
        {
            if(deliverProcess.DetailState== DeliverProcessState.DELIVER_SUCCESS && deliverProcess.CageCarId == this.CageCarId )
            {
                this.CageCarPackageCount++;
            }
            this.HandlingProcess.RemoveAll(p => p.DeliverProcessId == deliverProcess.Id);
            CheckState();
        }


        /// <summary>
        /// 处理当前笼车包裹数量，正在处理的流程数量和最大投递数量这件的关系。
        /// </summary>
        void CheckState()
        {
            if(this.State!= DeliverOutletState.FREE && this.State!= DeliverOutletState.FORBIDDEN && this.State!= DeliverOutletState.LOCKED)
            {
                if (this.CageCarPackageCount >= this.MaxPackageCount && this.HandlingProcess.Count == 0)
                {
                    this.State = DeliverOutletState.LOCKED;
                }
                else if(this.CageCarPackageCount + this.HandlingProcess.Count>=this.MaxPackageCount)
                {
                    this.State = DeliverOutletState.LOCKING;
                }
                else
                {
                    this.State = DeliverOutletState.DELIVER_AVAILABLE;
                }
            }
        }
    }

    public enum DeliverOutletType
    {
        /// <summary>
        /// 常规投递口
        /// </summary>
        NORMAL,
        /// <summary>
        /// 异常投递口
        /// </summary>
        ABNORMAL,
    }


    public enum DeliverOutletState
    {
        /// <summary>
        /// 空闲：没有绑定笼车，也没有被禁用
        /// </summary>
        FREE = 0,
        /// <summary>
        /// 投递可用：绑定了笼车，且可以接收新的包裹任务
        /// </summary>
        DELIVER_AVAILABLE = 1,
        /// <summary>
        /// 锁定中：绑定了笼车，不可接收新的包裹任务，已有任务不影响
        /// </summary>
        LOCKING = 2,
        /// <summary>
        /// 已锁定：绑定了笼车，
        /// </summary>
        LOCKED = 3,
        /// <summary>
        /// 禁用：没有绑定笼车
        /// </summary>
        FORBIDDEN = 4,

    }




    public class DeliverOutletLinkDeliverProcess : Entity
    {
        protected DeliverOutletLinkDeliverProcess() { }

        public DeliverOutletLinkDeliverProcess(string DeliverOutletID, string DeliverProcessId)
        {
            this.DeliverOutletId = DeliverOutletID;
            this.DeliverProcessId = DeliverProcessId;
        }
        /// <summary>
        /// 笼车Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string DeliverOutletId { get; protected set; } = "abc";
        /// <summary>
        /// 投递工作流
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string DeliverProcessId { get; protected set; } = "abc2";

        public override object[] GetKeys()
        {
            return new object[] { DeliverOutletId, DeliverProcessId };
        }
    }

}
