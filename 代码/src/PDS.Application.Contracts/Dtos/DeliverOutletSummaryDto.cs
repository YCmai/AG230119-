using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class DeliverOutletSummaryDto
    {
        /// <summary>
        /// 汇总分类
        /// </summary>
        public SummaryType Type { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Value { get; set; }
    }

    public enum SummaryType
    { 
        STATE=0,
        TYPE=1
    }
}
