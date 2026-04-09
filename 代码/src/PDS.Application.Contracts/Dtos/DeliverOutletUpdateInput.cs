using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class DeliverOutletUpdateInput:EntityDto<string>
    {
        /// <summary>
        /// 数据帧Id
        /// </summary>
        public string FrameId { get; set; }
        /// <summary>
        /// 投递口Id
        /// </summary>
        public string DeliverOutletId { get; set; }
        /// <summary>
        /// 最大包裹数量
        /// </summary>
        public int MaxPackageCount { get; set; }
    }
}
