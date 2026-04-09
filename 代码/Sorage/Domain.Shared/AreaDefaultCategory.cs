using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule.Domain.Shared
{
    //储位类型：
    public static class DefaultAreaCategory
    {
        /// <summary>
        /// 线边呼叫入库
        /// </summary>
        public const string LineCallInputArea = "LineCallInputArea";
        /// <summary>
        /// 线边呼叫出库
        /// </summary>
        public const string LineCallOutputArea = "LineCallOutputArea";
        /// <summary>
        /// 线边存储区域
        /// </summary>
        public const string LineCallStorageArea = "LineCallStorageArea";
    }
}
