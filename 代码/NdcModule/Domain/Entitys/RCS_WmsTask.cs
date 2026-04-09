using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_WmsTask : Entity
    {
        public int Id { get; set; }
        /// <summary>
        /// 调度任务编号
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public int TaskType { get; set; }
        /// <summary>
        /// 取货点
        /// </summary>
        public int PickupPoint { get; set; }
        /// <summary>
        /// 取货点高度
        /// </summary>
        public int PickupHeight { get; set; }
        /// <summary>
        /// 卸货点
        /// </summary>
        public int UnloadPoint { get; set; }
        /// <summary>
        /// 卸货点高度
        /// </summary>
        public int UnloadHeight { get; set; }
        /// <summary>
        /// 任务状态开始0，取货完成6，卸货完成9，结束22，取消30
        /// </summary>
        public TaskStatuEnum TaskStatus { get; set; }
       
        /// <summary>
        /// 任务优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? FinishTime { get; set; }


        

        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
