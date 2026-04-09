using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.StorageModule
{

  

    public class GetChildrenStoragePageInput: PagedResultRequestDto
    {
        /// <summary>
        /// 储位Id
        /// </summary>
        public string ParentStorageId { get; set; }
        /// <summary>
        /// 是否迭代
        /// </summary>
        public bool recursive { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WareHouseCode { get; set; }
        /// <summary>
        /// 区域编号
        /// </summary>
        public string AreaCode { get; set; }
    }

 
}
