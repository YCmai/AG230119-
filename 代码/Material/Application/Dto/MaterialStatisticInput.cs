using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule
{
    public class MaterialStatisticInput: PagedResultRequestDto
    {
        public string Sku { get; set; }

        public string WareHouseId { get; set; }
    }
}
