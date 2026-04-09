using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{
    public class StorageDto: EntityDto<string>
    {

        /// <summary>
        /// 父对象的Id
        /// </summary>
        public string ParentId { get;  set; }
        /// <summary>
        /// 绑定的agv地图节点，可空。
        /// </summary>
        public string MapNodeName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        ///  类型
        /// </summary>
        public string StorageType { get; set; }
        /// <summary>
        /// 尺寸信息（可空，具体与应用有关）
        /// </summary>
        public string SizeMes { get; set; }
        /// <summary>
        /// 是否为容器
        /// </summary>
        public bool IsContainer { get; set; }
    }
}
