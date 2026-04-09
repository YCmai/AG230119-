using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class PackageRegularFormatCreate
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RegularName { get; set; }
        /// <summary>
        /// 表达式内容
        /// </summary>
        public string RegularFormat { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>

        public string PackageSortId { get; set; }
    }
}
