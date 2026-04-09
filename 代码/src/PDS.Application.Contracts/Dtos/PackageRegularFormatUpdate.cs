using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class PackageRegularFormatUpdate:EntityDto<string>
    {
        public string FrameId { get; set; }
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
