using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using WMS.MaterialModule.Domain.Shared;

namespace WMS.MaterialModule.Domain
{
    /// <summary>
    /// 相当于出库记录
    /// </summary>
    public class MaterialRecord : AggregateRoot<Guid>
    {

        protected MaterialRecord() { }

        internal MaterialRecord(Guid Id, string materialInfoId, string BarCode, string StorageId,  int quatity, string batch, DateTime StoreTime,DateTime outputTime, string wareHouseId) : base(Id)
        {
            this.MaterialInfoId = materialInfoId;
            this.StoreTime = StoreTime;
            this.OutputTime = outputTime;
            this.BarCode = BarCode;
            this.StorageId = StorageId;
            this.Batch = batch;
            this.Quatity = quatity;
            this.WareHouseId = wareHouseId;
        }
        /// <summary>
        /// 物料信息Id（SKU）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string MaterialInfoId { get; private set; }
        /// <summary>
        /// 批次
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Batch { get; private set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime StoreTime { get; private set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime OutputTime { get; private set; }
        /// <summary>
        /// wms的唯一识别码。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string BarCode { get; private set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string StorageId { get; private set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quatity { get; private set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }

    }
}
