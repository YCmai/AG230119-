using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{

    public class AddMaterialInput
    {
        /// <summary>
        /// 批次号
        /// </summary>
        public string Batch { get; set; }
        /// <summary>
        /// SKU
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// wms的唯一识别码。实际跟id差不多。
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 总数量（可用数量+冻结数量+锁定数量）
        /// </summary>
        public int Quatity { get; set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        public string WareHouseId { get; set; }
    }
}
