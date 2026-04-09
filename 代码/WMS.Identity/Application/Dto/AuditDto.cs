using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.Identity
{
    public class AuditDto : EntityDto
    {
        public string UserName { get; set; }

        public int ExecutionDuration { get; set; }

        public DateTime ExecutionTime { get; set; }

        public string ClientIpAddress { get; set; }

        public string Url { get; set; }

        public string Exceptions { get; set; }

        public int? HttpStatusCode { get; set; }
    }
}
