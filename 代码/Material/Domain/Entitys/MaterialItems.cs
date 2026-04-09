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
    /// 物料项，是数据库记录物料的最小记录单位（按照每一个条码进行区分）
    /// </summary>
    public class MaterialItem : AggregateRoot<Guid>
    {

        protected MaterialItem() { }

        internal MaterialItem(Guid Id, string materialInfoId, string BarCode, string StorageId,  int Quatity, string batch, DateTime StoreTime, string wareHouseId) : base(Id)
        {
            this.MaterialInfoId = materialInfoId;
            this.StoreTime = StoreTime;
            this.BarCode = BarCode;
            this.StorageId = StorageId;
            this.Batch = batch;
            SetQuatity(Quatity, 0, 0);
            WareHouseId = wareHouseId;
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
        /// 总数量（可用数量+冻结数量+锁定数量）
        /// </summary>
        public int SumQuatity { get; private set; }
        /// <summary>
        /// 可用数量.(有可能是负数，因为盘点会只从这里数量，但总数量肯定大于0）。
        /// </summary>
        public int AvailableQuatity { get; private set; }
        /// <summary>
        /// 已冻结数量。
        /// </summary>
        public int FreezeQuatity { get; private set; }
        /// <summary>
        /// 已锁定数量。（出库时，先锁定物料）
        /// </summary>
        public int LockedQuatity { get; private set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }
        /// <summary>
        /// 设置数量
        /// </summary>
        /// <param name="AvailableQuatity"></param>
        /// <param name="FreezeQuatity"></param>
        /// <param name="LockedQuatity"></param>
        internal void SetQuatity(int AvailableQuatity, int FreezeQuatity, int LockedQuatity)
        {
            if (AvailableQuatity < 0 || FreezeQuatity < 0 || LockedQuatity < 0) throw new Exception("值不能小于0");
            
            this.AvailableQuatity = AvailableQuatity;
            this.FreezeQuatity = FreezeQuatity;
            this.LockedQuatity = LockedQuatity;
            this.SumQuatity = AvailableQuatity + FreezeQuatity + LockedQuatity;

        }


        internal void ChangeStorageRecord(string storageId)
        {
            this.StorageId = storageId;
        }

    }
}
