using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{

    public class GetAllLineCallOutputOrderPageInput: PagedResultRequestDto
    {
        /// <summary>
        /// 状态
        /// </summary>
        public LineCallOutputOrderState? State { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WarehouseCode { get; set; }
        /// <summary>
        /// sku
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 出库（下料）的储位
        /// </summary>
        public string OutputStorageId { get; set; }
    }
}
