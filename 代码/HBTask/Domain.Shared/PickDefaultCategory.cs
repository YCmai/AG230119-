using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.ProcessControlModule.Domain.Shared
{
    //储位类型：
    public static class PickDefaultCategory
    {
        /// <summary>
        /// 人工取货、拣货
        /// </summary>
        public const string Manual = "Manual";
        /// <summary>
        /// 线边呼叫
        /// </summary>
        public const string LineCall = "LineCall";  
        /// <summary>
        /// 货架到人
        /// </summary>
        public const string RackToPeople = "RackToPeople";  //上料储位
        /// <summary>
        /// 货箱到人
        /// </summary>
        public const string BoxToPeople = "BoxToPeople"; //下料储位
    }
}
