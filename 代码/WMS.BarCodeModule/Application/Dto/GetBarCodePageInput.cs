using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.BarCodeModule.Application
{

    public class GetBarCodePageInput: PagedResultRequestDto
    {
        public string? ParentBarCode { get; set; } = "";
        public string? BarCode { get; set; } = "";
    }
}
