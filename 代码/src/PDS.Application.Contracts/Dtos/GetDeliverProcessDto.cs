using System;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class GetDeliverProcessDto:EntityDto<string>
    {
        /// <summary>
        /// 包裹编码
        /// </summary>
        public string PackageId { get; set; }
        /// <summary>
        /// 来源站点
        /// </summary>
        public string Station { get; set; }
        /// <summary>
        /// 搬运设备
        /// </summary>
        public string AgvId { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? CloseTime { get; set; }
    }
}
