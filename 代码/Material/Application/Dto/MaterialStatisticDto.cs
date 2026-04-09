using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule
{
    public class MaterialStatisticDto
    {
        public string WareHouseId { get; set; }

        public string Sku { get; set; }

        public int SumQuatity { get; set; }

        public int AvailableQuatity { get; set; }

        public int FreezeQuatity { get; set; }

        public int LockedQuatity { get; set; }

        public string MaterialName { get; set; }
    }
}
