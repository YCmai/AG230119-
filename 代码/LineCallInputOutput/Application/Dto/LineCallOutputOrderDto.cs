using System;
using Volo.Abp.Application.Dtos;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{
    public class LineCallOutputOrderDto : EntityDto<Guid>
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public LineCallOutputOrderState State { get;  set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string ReMark { get; set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime CloseTime { get; set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 存储储位（系统分配）
        /// </summary>
        public string StoreStorageId { get; set; }
        /// <summary>
        /// 出库储位（呼叫时给出）
        /// </summary>
        public string OutputStorageId { get; set; }
        /// <summary>
        /// 需要出库的物料的SKU
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 物料唯一条码(系统分配）
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WarehouseCode { get; set; }
        /// <summary>
        /// 关联的线边出库任务的Id
        /// </summary>
        public Guid LineCallOutputTaskId { get; set; }

    }
}
