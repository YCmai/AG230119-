using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule
{
    public class UpdateAreaInput
    {
        /// <summary>
        /// 区域名称
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }
    }
}
