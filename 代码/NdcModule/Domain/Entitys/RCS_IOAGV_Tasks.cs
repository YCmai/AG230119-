using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_IOAGV_Tasks : Entity
    {
        public int Id { get; set; }
        /// <summary>
        /// 任务类型：ArrivalNotify(到达通知), PassCheck(通行检查), PassComplete(通行完成)
        /// </summary>
        public string TaskType { get; set; }
        /// <summary>
        /// 任务状态：Pending(待处理), Completed(已完成)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// IO设备IP地址
        /// </summary>
        public string DeviceIP { get; set; }
        /// <summary>
        ///  IO信号地址
        /// </summary>
        public string SignalAddress { get; set; }

        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        ///  最后更新时间
        /// </summary>
        public DateTime? LastUpdatedTime { get; set; }


        public string TaskId { get; set; }


        public bool Value { get; set; }




        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
