using System;
using Volo.Abp.Application.Dtos;


namespace WMS.R2POutputModule.Domain
{

    public class R2POutputTaskDto : EntityDto<Guid>
    {

        public R2POutputTaskState State { get; set; }
        /// <summary>
        /// 分配的处理这个流程的设备Id
        /// </summary>
        public string DeviceId { get; set; }
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
