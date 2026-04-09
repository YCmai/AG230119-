using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_UserTasks : Entity
    {
        public int ID { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatuEnum taskStatus { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? executedTime { get; set; }


        /// <summary>
        /// 设备管理器任务ID
        /// </summary>
        public string? runTaskId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 是否要执行
        /// </summary>
        public bool executed { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? creatTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }


        /// <summary>
        /// 任务类型
        /// </summary>
        public TaskType taskType { get; set; }

        /// <summary>
        /// 请求编号
        /// </summary>
        public string? requestCode { get; set; }


        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }


        /// <summary>
        /// 执行任务的agv
        /// </summary>
        public string? robotCode { get; set; }

        /// <summary>
        /// 起点
        /// </summary>
        public string? sourcePosition { get; set; }

        /// <summary>
        /// 目标点
        /// </summary>
        public string? targetPosition { get; set; }


        public bool IsCancelled { get; set; }



        public override object[] GetKeys()
        {
            return new object[] { ID };
        }

        public enum TaskType
        {
            /// <summary>
            /// 上料架到锅炉起升架
            /// </summary>
            [Display(Name = "上料架到锅炉起升架")]
            上料 = 1,

            /// <summary>
            /// 下料线到空储位
            /// </summary>
            [Display(Name = "下料线到空储位")]
            下料 = 2,

            /// <summary>
            /// 空储位到上料架
            /// </summary>
            [Display(Name = "空储位到上料架")]
            AssemblyToPlatingEmpty = 3
        }
    }
}
