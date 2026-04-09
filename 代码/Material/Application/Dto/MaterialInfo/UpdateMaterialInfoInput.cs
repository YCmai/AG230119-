using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule
{
    public class UpdateMaterialInfoInput
    {
        public string Name { get; set; }
        public bool IsContainer { get; set; }
        public string Category { get; set; }
        public string Describtion { get; set; }

        public string SizeMess { get; set; }
    }
}
