using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{

    public class GetAllMaterialPageInput: PagedResultRequestDto
    {
        public string Sku { get; set; }
        public string StorageId { get; set; }
        /// <summary>
        /// 所在仓库
        /// </summary>
        public string WhereHouseId { get; set; }
        /// <summary>
        /// 只查询可用的
        /// </summary>
        public bool OnlyAvailable { get; set; }
    }
}
