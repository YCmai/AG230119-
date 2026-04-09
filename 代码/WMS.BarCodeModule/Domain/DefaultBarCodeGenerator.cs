using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace WMS.BarCodeModule.Domain
{
    internal class DefaultBarCodeGenerator : IBarCodeGenerator, ISingletonDependency
    {
        private readonly IGuidGenerator _guidGenerator;

        public DefaultBarCodeGenerator(IGuidGenerator guidGenerator)
        {
            _guidGenerator = guidGenerator;
        }
        public string Create(string fatherCode)
        {
            return _guidGenerator.Create().ToString();
        }
    }
}
