using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class DeliverOutletGetListInput:EntityDto<string>
    {
        /// <summary>
        /// 查询状态
        /// </summary>
        public DeliverOutletState? State { get; set; }
        /// <summary>
        /// 投递口类型
        /// </summary>
        public DeliverOutletType? DeliverType { get; set; }
    }
}
