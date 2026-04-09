using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule
{
    public class CreateMaterialInfoInput
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public bool IsContainer { get; set; }
        public string Category { get; set; }
        public string Describtion { get; set; }

        public string SizeMess { get; set; }
    }
}
