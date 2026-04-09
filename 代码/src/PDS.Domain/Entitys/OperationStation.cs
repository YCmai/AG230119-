using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    /// <summary>
    /// 包裹操作站
    /// </summary>
    public class OperationStation: BasicAggregateRoot<string>  //todo,包裹操作点也可能是人上线的，robotId实际是人，此时怎么通知人去投递？
    {
        protected OperationStation() { }
        /// <summary>
        /// 包裹操作站点（上料）
        /// </summary>
        /// <param name="Id">操作唯一Id</param>
        /// <param name="BindingMapNodeName">与地图映射的节点</param>
        /// <param name="MaxAgvCount">最大agv数量</param>
        /// <param name="loadRobotId">上料设备的Id（机械臂，传输带）</param>
        public OperationStation(string Id, string BindingMapNodeName, int MaxAgvCount,string loadRobotId)
        {
            this.Id = Id;
            this.MapNodeName = BindingMapNodeName;
            this.MaxAgvCount = MaxAgvCount;
            this.State = OperationStationState.OUTLINE;  //初始状态是离线
            this.LoadRobotId = loadRobotId;
        }

        public string LoadRobotId { get; protected set; }
        /// <summary>
        /// 对应的地图节点名称
        /// </summary>
        public string MapNodeName { get; protected set; }
        /// <summary>
        /// 操作点最大Agv数量
        /// </summary>
        public int MaxAgvCount { get; protected set; }
        /// <summary>
        /// 操作站点状态
        /// </summary>
        public OperationStationState State { get; protected set; }
        /// <summary>
        /// 操作站上线时间点
        /// </summary>
        public DateTime? SignInTime { get; protected set; }
        /// <summary>
        /// 操作站下线时间点
        /// </summary>
        public DateTime? SignOutTime { get; protected set; }


        /// <summary>
        /// 最后一次登录信号的时间
        /// </summary>
        public DateTime? LastSignalTime { get;  set; }

        public void ClaimState(OperationStationState NewState)
        {
            if(this.State != NewState)
            {
                this.State = NewState;
                switch(this.State)
                {
                    case OperationStationState.ONLINE:
                        this.SignInTime = DateTime.Now;
                        this.SignOutTime = null;
                        break;

                    case OperationStationState.OUTLINE:
                        this.SignOutTime = DateTime.Now;
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetMaxAgvCount(int agvCount)
        {
            this.MaxAgvCount = agvCount;
        }
    }



    public enum OperationStationState
    {
        /// <summary>
        /// 在线（在线后，agv会去站点排队）
        /// </summary>
        ONLINE,
        /// <summary>
        /// 离线。
        /// </summary>
        OUTLINE,
        /// <summary>
        /// 禁用。系统不再使用此站点。
        /// </summary>
        ABFORBIDDEN,
    }

}
