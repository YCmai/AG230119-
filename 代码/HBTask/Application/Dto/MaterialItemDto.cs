using System;
using Volo.Abp.Application.Dtos;


namespace WMS.WaveOrderModule.Domain
{

    public class MaterialItemDto : EntityDto<Guid>
    {
        protected MaterialItemDto() { }

        /// <summary>
        /// SKU
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime StoreTime { get; set; }
        /// <summary>
        /// wms的唯一识别码。实际跟id差不多。
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 总数量（可用数量+冻结数量+锁定数量）
        /// </summary>
        public int SumQuatity { get; set; }
        /// <summary>
        /// 可用数量
        /// </summary>
        public int AvailableQuatity { get; set; }
        /// <summary>
        /// 已冻结数量。
        /// </summary>
        public int FreezeQuatity { get; set; }
        /// <summary>
        /// 已锁定数量。
        /// </summary>
        public int LockedQuatity { get; set; }
        /// <summary>
        /// 应用数据
        /// </summary>
        public string AppData { get; set; }
    }
}
