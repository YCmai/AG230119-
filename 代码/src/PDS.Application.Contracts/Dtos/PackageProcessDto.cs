using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS.Dtos
{
    public class PackageProcessDto:EntityDto<string>
    {
        /// <summary>
        /// 操作点
        /// </summary>
        public string OperationStationId { get; protected set; }
        /// <summary>
        /// 目标投递口
        /// </summary>
        public string DeliverOutletId { get; protected set; }
        /// <summary>
        /// 投递口类型
        /// </summary>
        public DeliverOutletType DeliverOutletType { get; protected set; }
        /// <summary>
        /// 目标笼车
        /// </summary>
        public string CageCarId { get; protected set; }
        /// <summary>
        /// 包裹
        /// </summary>
        public string PackageId { get; protected set; }
        /// <summary>
        /// 执行投递流程的Agv
        /// </summary>
        public string AgvId { get; protected set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreationTime { get; protected set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? CloseTime { get; protected set; }
        /// <summary>
        /// 投递流程的状态
        /// </summary>
        public DeliverProcessState DetailState { get; protected set; } = DeliverProcessState.WAITING_AGV;
        /// <summary>
        /// 包裹放置时间
        /// </summary>
        public DateTime? PackageLoadTime { get; protected set; }
    }
}
