using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{

    public class GetAllMaterialRecordPageInput: PagedResultRequestDto
    {
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// SKU
        /// </summary>
        public string Sku { get; set; }
        /// <summary>
        /// 储位
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        public string WhereHouseId { get; set; }
    }
}
