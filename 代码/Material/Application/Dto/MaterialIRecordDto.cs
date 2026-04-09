using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using WMS.MaterialModule.Domain.Shared;

namespace WMS.MaterialModule.Domain
{
    /// <summary>
    /// 物料记录
    /// </summary>
    public class MaterialRecordDto: EntityDto<Guid>
    {
        /// <summary>
        /// 物料信息Id（SKU）
        /// </summary>
        public string MaterialInfoId { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string Batch { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime StoreTime { get; set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime OutputTime { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// wms的唯一识别码。
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 物料类别
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quatity { get; set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        public string WareHouseId { get; set; }
        /// <summary>
        /// 尺寸信息
        /// </summary>
        public string SizeMess { get; set; }
        /// <summary>
        /// 是否为容器
        /// </summary>
        public bool InContainer { get; set; }

    }
}
