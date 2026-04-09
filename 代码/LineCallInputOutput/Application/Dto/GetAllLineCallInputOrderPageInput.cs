using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{

    public class GetAllLineCallInputOrderPageInput: PagedResultRequestDto
    {
        /// <summary>
        /// 状态
        /// </summary>
        public LineCallInputOrderState? State { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WarehouseCode { get; set; }
        /// <summary>
        /// sku
        /// </summary>
        public string SKU { get; set; }
        /// <summary>
        /// 入库（上料）的储位
        /// </summary>
        public string InputStorageId { get; set; }
    }
}
