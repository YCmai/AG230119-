using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class GetPackageProcessSummaryCount
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public long TotalCount { get { return NormalCount + AbnormalCount; } }
        /// <summary>
        /// 正常投递
        /// </summary>
        public int NormalCount { get; set; }
        /// <summary>
        /// 异常投递
        /// </summary>
        public int AbnormalCount { get; set; }
    }
}
