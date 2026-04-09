using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.StorageModule
{
    public class GetWareHousePageRequest: PagedResultRequestDto
    {
        public string WareHouseId { get; set; }
    }
}
