using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_ApiTask : Entity
    {
        public int Id { get; set; }
        /// <summary>
        /// 调度任务编号
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// 任务类型0取货，1卸货
        /// </summary>
        public int TaskType { get; set; }

        public bool Excute { get; set; }

        public string Message { get; set; }

        public DateTime CreateTime { get; set; }
        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
