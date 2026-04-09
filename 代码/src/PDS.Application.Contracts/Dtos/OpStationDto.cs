using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class OperationStationDto:EntityDto<string>
    {
        /// <summary>
        /// 对应的地图节点名称
        /// </summary>
        public string MapNodeName { get; set; }
        /// <summary>
        /// 操作站点状态
        /// </summary>
        public string State { get; set; } 
        /// <summary>
        /// 当前上线的用户
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 当前上线用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 上线时间（单位s）
        /// </summary>
        public string SignInTime { get; set; }
        /// <summary>
        /// 当天的的包裹数量
        /// </summary>
        public int PackageCount { get; set; }

        /// <summary>
        /// 操作点最大Agv数量
        /// </summary>
        public int MaxAgvCount { get; set; }

        /// <summary>
        /// 操作站下线时间点
        /// </summary>
        public string SignOutTime { get; set; }
    }

}
