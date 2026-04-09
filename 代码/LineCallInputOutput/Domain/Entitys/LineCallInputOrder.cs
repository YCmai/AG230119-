using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace WMS.LineCallInputModule.Domain
{

    /// <summary>
    /// 线边呼叫入库单（移库呼叫单）
    /// </summary>
    public class LineCallInputOrder : AggregateRoot<Guid>
    {

        protected LineCallInputOrder() { }

        public LineCallInputOrder(Guid Id, string reMark, Guid pickUserId, Guid creatorId, string putonStorageId, string sku, string barCode,string warehouseCode) : base(Id)
        {
            this.State = LineCallInputOrderState.HasPutOn; //对于线边呼叫入库，呼叫即为已经上架，也就是物料放在入库储位上，才呼叫。
            this.ReMark = reMark;
            this.CreatorId = creatorId;
            this.CreationTime = DateTime.Now;
            this.InputStorageId = putonStorageId;
            this.SKU = sku;
            this.BarCode = barCode;
            this.WarehouseCode = warehouseCode;
        }
        /// <summary>
        /// 线边呼叫入库单状态
        /// </summary>
        public LineCallInputOrderState State { get; private set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string ReMark { get; set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? CloseTime { get; private set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; private set; }
        /// <summary>
        /// 上料储位编号
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string InputStorageId { get; private set; }
        /// <summary>
        /// 分配的储位
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string StoreStorageId { get; internal set; }
        /// <summary>
        /// sku
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string SKU { get; private set; }
        /// <summary>
        /// 物料唯一条码
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string BarCode { get; private set; }
        /// <summary>
        /// 关联的线边呼叫入库任务Id
        /// </summary>
        public Guid LineCallInputTaskId { get; private set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WarehouseCode { get; private set; }
        



        internal void SetState(LineCallInputOrderState NewState)
        {
            if (this.State != NewState)
            {
                this.State = NewState;
                if (this.State == LineCallInputOrderState.Cancel || this.State == LineCallInputOrderState.Finished)
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }

        internal void BindingTask (Guid taskId)
        {
            this.LineCallInputTaskId = taskId;
        }
    }


    public enum LineCallInputOrderState : int
    {
        /// <summary>
        /// 已上架。(临时态））
        /// </summary>
        HasPutOn = 0,
        /// <summary>
        /// 正在移储（临时态）
        /// </summary>
        Delivering = 1,
        /// <summary>
        /// 已完成（最终态）
        /// </summary>
        Finished = 2,
        /// <summary>
        /// 取消（最终态）
        /// </summary>
        Cancel = 3,
    }
}
