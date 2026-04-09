using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.LineCallInputModule.Application
{
    public class LineCallInputOrderCreateInput
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string ReMark { get; set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        /// 上料储位Id
        /// </summary>
        public string PutonStorageId { get; set; }
        /// <summary>
        /// sku
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 物料唯一条码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WarehouseId { get;  set; }

    }
}
