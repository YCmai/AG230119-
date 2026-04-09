using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule.Domain.Shared
{

    //默认储位结构：
    //1.线边呼叫  ：仓库->区域->料架->普通储位



    //储位类型：
    public static class StorageDefaultCategory
    {
        /// <summary>
        /// 普通储位
        /// </summary>
        public const string GeneralStorage = "GeneralStorage";

        /// <summary>
        /// 叉车栈板
        /// </summary>
        public const string ForkPallet = "ForkPallet";

        /// <summary>
        /// 料架
        /// </summary>
        public const string Rack = "Rack";
    }
}


