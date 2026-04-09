using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class DeliverOutletDeleteInput:EntityDto<string>
    {
        /// <summary>
        /// 数据帧Id
        /// </summary>
        public string FrameId { get; set; }
    }
}
