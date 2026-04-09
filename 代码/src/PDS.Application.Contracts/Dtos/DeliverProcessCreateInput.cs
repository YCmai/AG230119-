using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class DeliverProcessCreateInput : EntityDto
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 包裹所在的操作站
        /// </summary>
        public string OperationStationId { get; set; }
        /// <summary>
        /// 包裹编号
        /// </summary>
        public string PackageCode { get; set; }
        /// <summary>
        /// 包裹的扩展信息
        /// </summary>
        public string PackageExtInfo { get; set; }
        /// <summary>
        /// 包裹分类描述。如果是空，系统则会通过包裹信息来确定分类。
        /// </summary>
        public string PackageSortDecribtion { get; set; }
    }
}
