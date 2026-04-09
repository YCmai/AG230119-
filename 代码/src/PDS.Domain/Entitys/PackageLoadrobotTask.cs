using PDS.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{

    public class PackageLoadrobotTask : BasicAggregateRoot<string>
    {
        protected PackageLoadrobotTask() { }

        public PackageLoadrobotTask(string Id, string RobotId,string DeliverProcessId)
        {
            this.Id = Id;
            this.CreationTime = DateTime.Now;
            this.RobotId = RobotId;
            this.DeliverProcessId = DeliverProcessId;
        }


        /// <summary>
        /// 流程上下文
        /// </summary>
        public string DeliverProcessId { get; protected set; }
        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreationTime { get; private set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime CloseTime { get; private set; }

        public PackageLoadrobotTaskState TaskState { get; protected set; } = PackageLoadrobotTaskState.NotStart;
        /// <summary>
        /// 使用的设备id
        /// </summary>
        public string RobotId { get; protected set; }



        public void ClaimTaskState(PackageLoadrobotTaskState NewState)
        {
            if (this.TaskState != NewState)
            {
                var oldState = this.TaskState;
                this.TaskState = NewState;

                switch (NewState)
                {
                    case PackageLoadrobotTaskState.ExcuteFault:
                    case PackageLoadrobotTaskState.Finished:
                        this.CloseTime = DateTime.Now;
                        break;

                    default:
                        //不用处理
                        break;
                }

                AddLocalEvent(new EntityStateChangingEvent<PackageLoadrobotTask, PackageLoadrobotTaskState>(this, oldState, NewState));
            }
        }
    }


    public enum PackageLoadrobotTaskState
    {
        /// <summary>
        /// 当前任务还没有开始
        /// </summary>
        NotStart,
        /// <summary>
        ///  当前任务已经完成;
        /// </summary>
        Finished,
        /// <summary>
        ///  执行失败且不再执行
        /// </summary>
        ExcuteFault,
    }
}
