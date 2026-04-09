using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_IOSignals : Entity
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        /// <summary>
        /// // 错误状态-1（如连接失败、读取失败等）关闭状态 (0)开启状态 (1)
        /// </summary>
        public int Value { get; set; }
        public virtual RCS_IODevices Device { get; set; }




        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
