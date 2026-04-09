using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule
{
    public class StorageCreateInput
    {
        /// <summary>
        /// 需要添加的储位编码
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 父对象的Id
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 绑定的agv地图节点，可空。
        /// </summary>
        public string MapNodeName { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        ///  类型
        /// </summary>
        public string StorageType { get; set; }
        /// <summary>
        /// 储位别名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 尺寸信息（可空，具体与应用有关）
        /// </summary>
        public string SizeMes { get; set; }
        /// <summary>
        /// 应用数据1
        /// </summary>
        public string AppData1 { get; set; }
        /// <summary>
        /// 应用数据2
        /// </summary>
        public string AppData2 { get; set; }
        /// <summary>
        /// 储位起始高度
        /// </summary>
        public int StartHeight { get; set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WareHouseId { get; set; }
        /// <summary>
        /// 区域编号
        /// </summary>
        public string WareHouseAreaCode { get; set; }

    }
}
