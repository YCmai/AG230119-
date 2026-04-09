using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class GetDeliverProcessPageRequest: PagedResultRequestDto
    {
        /// <summary>
        /// 投递口Id
        /// </summary>
        public string DeliverOutletId { get; set; }
    }
}
