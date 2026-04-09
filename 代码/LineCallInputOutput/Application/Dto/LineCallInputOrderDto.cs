using System;
using Volo.Abp.Application.Dtos;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{
    public class LineCallInputOrderDto : EntityDto<Guid>
    {
        /// <summary>
        /// 线边呼叫入库单状态
        /// </summary>
        public LineCallInputOrderState State { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string ReMark { get; set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? CloseTime { get; set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 已上料的储位Id
        /// </summary>
        public string InputStorageId { get; set; }
        /// <summary>
        /// sku
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 物料唯一条码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 对应的任务
        /// </summary>
        public Guid LineCallInputTaskId { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WarehouseCode { get; private set; }

    }
}
