using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.StorageModule
{

    public class GetRootStoragePageInput: PagedResultRequestDto
    {
        public string WareHouseCode { get; set; }
        public string AreaCode { get; set; }
    }
}
