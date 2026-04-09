using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace WMS.LineCallInputModule.Domain
{

    public class LineCallOutputOrder : AggregateRoot<Guid>
    {
        protected LineCallOutputOrder() { }


        public LineCallOutputOrder(Guid Id, string reMark, Guid creatorId,string sku, string outputStorageId,  string warehouseCode) : base(Id)
        {
            this.State = LineCallOutputOrderState.Notstart;
            this.ReMark = reMark;
            this.CreatorId = creatorId;
            this.SKU = sku;
            this.CreationTime = DateTime.Now;
            this.OutputStorageId = outputStorageId;
            this.WarehouseCode = warehouseCode;
        }
        /// <summary>
        /// 状态
        /// </summary>
        public LineCallOutputOrderState State { get; private set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string ReMark { get; set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime CloseTime { get; private set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; private set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; private set; }
        /// <summary>
        /// 存储储位（系统分配）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string StoreStorageId { get; private set; }
        /// <summary>
        /// 出库储位（呼叫时给出）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string OutputStorageId { get; internal set; }
        /// <summary>
        /// 需要出库的物料的SKU
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string SKU { get; private set; }
        /// <summary>
        /// 物料唯一条码(系统分配）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string BarCode { get; internal set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WarehouseCode { get; private set; }
        /// <summary>
        /// 关联的线边出库任务的Id
        /// </summary>
        public Guid LineCallOutputTaskId { get;private set; }

        internal void SetState(LineCallOutputOrderState NewState)
        {
            if (this.State != NewState)
            {
                this.State = NewState;
                if (this.State == LineCallOutputOrderState.Cancel || this.State == LineCallOutputOrderState.Finished)
                {
                    this.CloseTime = DateTime.Now;
                }
            }
        }

        internal void ComfirmStoreStorageId(string storeStorageId)
        {
            this.StoreStorageId= storeStorageId;
        }

        internal void BindingTask(Guid lineCallOutputTaskId)
        {
            this.LineCallOutputTaskId = lineCallOutputTaskId;
        }

    }


    public enum LineCallOutputOrderState : int
    {
        /// <summary>
        /// 未开始。(临时态））
        /// </summary>
        Notstart = 0,
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
