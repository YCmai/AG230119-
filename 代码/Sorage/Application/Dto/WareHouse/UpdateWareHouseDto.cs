using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule
{
    public class UpdateWareHouseDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}
