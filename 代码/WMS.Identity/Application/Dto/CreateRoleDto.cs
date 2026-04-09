using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace WMS.Identity
{
    public class CreateRoleDto : IdentityRoleCreateOrUpdateDtoBase
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public CreateRoleDto()
        {
            ExtraProperties.Add("Description", Description);
        }
    }
}
