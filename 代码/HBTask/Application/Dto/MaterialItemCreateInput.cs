using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.WaveOrderModule.Application
{
    public class MaterialItemCreateInput
    {
        public Guid Guid { get; set; }
        public string SKU { get; set; }

        public string AppData { get; set; }
        /// <summary>
        /// 尺寸信息；
        /// </summary>
        public string SizeMess { get; private set; }
        /// <summary>
        /// 是否为容器
        /// </summary>
        public bool IsContainer { get; private set; }

        /// <summary>
        /// wms的唯一识别码。实际跟id差不多。
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        public string StorageId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describtion { get; set; }
        /// <summary>
        /// 物料类别
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 总数量（可用数量+冻结数量+锁定数量）
        /// </summary>
        public int SumQuatity { get; set; }

        /// <summary>
        /// 可用数量
        /// </summary>
        public int AvailableQuatity { get; set; }
        /// <summary>
        /// 已冻结数量。
        /// </summary>
        public int FreezeQuatity { get; set; }
        /// <summary>
        /// 已锁定数量。
        /// </summary>
        public int LockedQuatity { get; set; }
    }
}
