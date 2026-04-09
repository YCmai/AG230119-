using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.StorageModule
{
    public class StorageDto : EntityDto<string>
    {

        /// <summary>
        /// 留用数据；根据具体的应用来使用。
        /// </summary>
        public string AppData1 { get; set; }
        /// <summary>
        /// 留用数据；根据具体的应用来使用
        /// </summary>
        public string AppData2 { get; set; }
        /// <summary>
        /// 绑定的agv地图节点，可空。
        /// </summary>
        public string MapNodeName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; protected set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid CreatorId { get; protected set; }
        /// <summary>
        ///  储位类别
        /// </summary>
        public string Category { get; protected set; }
        /// <summary>
        /// 储位名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 尺寸信息（可空，具体与应用有关，长宽高使用字母x隔开）
        /// </summary>
        public string SizeMes { get; protected set; }
        /// <summary>
        /// 储位的起始高度（mm，可空）（常用于控制叉臂叉取货物）
        /// </summary>
        public int StartHeight { get; protected set; }
        /// <summary>
        /// 所属仓库Id
        /// </summary>
        public string WareHouseId { get; private set; }
        /// <summary>
        /// 所属区域Id
        /// </summary>
        public string WareHouseIdAreaCode { get; private set; }
        /// <summary>
        /// 当前节点的物料数量
        /// </summary>
        public int CurrentNodeMaterialCount { get; protected set; }
        /// <summary>
        /// 父储位的Id
        /// </summary>
        public string ParentId { get; internal set; }
    }
}
