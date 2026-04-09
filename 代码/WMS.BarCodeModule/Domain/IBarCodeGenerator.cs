using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BarCodeModule.Domain
{
    public interface IBarCodeGenerator
    {
        string Create(string fatherCode);
    }
}
