using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_TaskErrRecord : Entity
    {
        public int Id { get; set; }
        /// <summary>
        /// 异常备注
        /// </summary>
        public string TaskRemark { get; set; }
      
        /// <summary>
        /// 任务类型
        /// </summary>
        public int TaskType { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime CreataTime { get; set; }



        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
