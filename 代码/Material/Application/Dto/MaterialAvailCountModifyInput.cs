using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{

    public class MaterialAvailCountModifyInput
    {
        public string BarCode { get; set; }
        public int OldCount { get; set; }
        public int NewCount { get; set; }
        public string WareHouseId { get; set; }
    }
}
