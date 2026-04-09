using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS.Dtos
{
    public class PackageProcessPageRequest: PagedResultRequestDto
    {
        public string Code { get; set; }
    }
}
