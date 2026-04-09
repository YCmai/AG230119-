using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule
{
    public class AddAreaInput
    {
        /// <summary>
        /// 仓库ID
        /// </summary>
        public string wareHouseId { get; set; }
        /// <summary>
        /// 区域编码
        /// </summary>

        public string Code { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

    }
}
