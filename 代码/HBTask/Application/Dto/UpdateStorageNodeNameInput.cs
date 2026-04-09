using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule.Application
{
    public class UpdateStorageNodeNameInput
    {
        /// <summary>
        /// 储位大小
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 是否递归获取子项。
        /// </summary>
        public string NodeName { get; set; }
    }
}
