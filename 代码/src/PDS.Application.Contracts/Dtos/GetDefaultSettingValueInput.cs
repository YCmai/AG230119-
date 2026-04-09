using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class GetDefaultSettingValueInput : PagedResultRequestDto
    {
        public string SettingName { get; set; }
    }
}
