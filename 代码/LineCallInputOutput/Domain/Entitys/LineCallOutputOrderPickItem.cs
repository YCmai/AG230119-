using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace WMS.LineCallInputModule.Domain
{

    public class LineCallOutputOrderPickItem : AggregateRoot<Guid>
    {
        protected LineCallOutputOrderPickItem()
        {

        }

        /// <summary>
        /// 创建拣货明细
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="sku"></param>
        /// <param name="pickCategory"></param>
        /// <param name="quantity"></param>
        /// <param name="materialItemId"></param>
        public LineCallOutputOrderPickItem(Guid Id, string sku, string pickCategory, int quantity, Guid materialItemId) : base(Id)
        {
            if (quantity <= 0) throw new Exception("拣货数量不可以小于等于0");
            this.SKU = sku;
            this.PickCategory = pickCategory;
            this.State = WaveOrderPickItemState.WaitStart;

            this.Quantity = quantity;
            this.MaterialItemId = materialItemId;
        }

        /// <summary>
        /// 创建缺货的拣货明细
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="sku"></param>
        /// <param name="pickCategory"></param>
        public LineCallOutputOrderPickItem(Guid Id,string sku, string pickCategory):base(Id)
        {
            this.SKU = sku;
            this.PickCategory = pickCategory;
            this.State = WaveOrderPickItemState.Lack;
        }

        /// <summary>
        /// sku
        /// </summary>
        public string SKU { get; private set; }
        /// <summary>
        /// 拣货数量
        /// </summary>
        public int Quantity { get; private set; }
        /// <summary>
        /// 拣货对应的物料Id。如果没有值，表示物料是空的。
        /// </summary>
        public Guid? MaterialItemId { get; private set; }
        /// <summary>
        /// 拣货状态
        /// </summary>
        public WaveOrderPickItemState State { get; private set; }
        /// <summary>
        /// 拣货方式
        /// </summary>
        public string PickCategory { get; private set; }
        /// <summary>
        /// 拣货时间
        /// </summary>
        public DateTime PickTime { get; private set; }

        public Guid? ProcessControllerId { get; private set; }


        public void PickFinishedConfirm(int ActualPickCount)
        {
            if (this.State != WaveOrderPickItemState.Picking) throw new Exception("只有状态为picking时才能设置为完成");
            if (ActualPickCount != this.Quantity) throw new Exception("实际拣货数量不等于拣货明细数量");
            this.State = WaveOrderPickItemState.Finished;
        }

        public void PickStartConfirm(Guid processControllerId)
        {
            if (this.State != WaveOrderPickItemState.WaitStart) throw new Exception("只有状态为picking时才能设置为完成");
            this.ProcessControllerId = processControllerId;
            this.State = WaveOrderPickItemState.Picking;
        }

        /// <summary>
        /// 确定物料.当前状态是“缺料”才可以确认物料，确认物料后，状态会变为waitStart；
        /// </summary>
        /// <param name="MaterialItemId"></param>
        public void ConfirmMaterial(Guid materialItemId,int num)
        {
            if (this.State != WaveOrderPickItemState.Lack) throw new Exception("只有状态为picking时才能设置为完成");
            if (num <= 0) throw new Exception("拣料数量不得小于等于为0");
            this.MaterialItemId = materialItemId;
            this.Quantity = num;
            this.State = WaveOrderPickItemState.WaitStart;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        public void ModifyPickQuantity(int num)
        {
            if (this.State == WaveOrderPickItemState.Finished) throw new Exception("拣料已经完成，无法修改数量");  
            //todo，拣料中修改好像也不是很好。如果是已经已经弹窗提示拣料数量了，还没确认拣料完成，那么如果此时修改数量，操作员可能不好操作。
            if (num <= 0) throw new Exception("拣料数量不得小于等于为0");
            this.Quantity = num;
        }
    }


    public enum WaveOrderPickItemState : int
    {
        /// <summary>
        /// 欠料，无法拣货，此时物料id为空，或者对应物料
        /// </summary>
        Lack = 0,
        /// <summary>
        /// 等待开始
        /// </summary>
        WaitStart = 1,
        /// <summary>
        /// 拣货中，已经触发了控制任务
        /// </summary>
        Picking = 2,
        /// <summary>
        /// 拣选已完成
        /// </summary>
        Finished = 3,
    }
}
