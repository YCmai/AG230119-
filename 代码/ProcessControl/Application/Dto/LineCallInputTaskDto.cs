using System;
using Volo.Abp.Application.Dtos;


namespace WMS.LineCallProcessTaskModule.Domain
{

    public class LineCallInputTaskDto : EntityDto<Guid>
    {

        public LineCallInpputTaskState State { get; set; }
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
