using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.R2POutputModule.Domain;

namespace WMS.R2POutputModule.Application
{
    public class R2POutputTaskCreateInput
    {
        public Guid TaskId { get; set; }
        /// <summary>
        /// 取货节点
        /// </summary>
        public string LoadNodeName { get; set; }
        /// <summary>
        /// 卸货节点
        /// </summary>
        public string UnloadNodeName { get; set; }
    }
}
