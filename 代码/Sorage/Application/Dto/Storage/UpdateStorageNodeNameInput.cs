using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule
{
    public class UpdateStorageInput
    {
        public string Id { get; set; }
        /// <summary>
        /// 储位名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 尺寸信息（可空，具体与应用有关，长宽高使用字母x隔开）
        /// </summary>
        public string SizeMes { get; set; }
        /// <summary>
        /// 储位的起始高度（mm，可空）（常用于控制叉臂叉取货物）
        /// </summary>
        public int StartHeight { get; set; }
        /// <summary>
        /// 地图节点
        /// </summary>
        public string MapNodeName { get; set; }
        /// <summary>
        /// 储位类型
        /// </summary>
        public string Category { get; set; }

        public string AppData1 { get; set; }
        public string AppData2 { get; set; }
    }
}
