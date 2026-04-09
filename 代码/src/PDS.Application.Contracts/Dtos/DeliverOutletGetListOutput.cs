using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class DeliverOutletGetListOutput
    {
        /// <summary>
        /// 结果数量
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        public List<DeliverOutletDto> Items { get; set; }
    }
}
