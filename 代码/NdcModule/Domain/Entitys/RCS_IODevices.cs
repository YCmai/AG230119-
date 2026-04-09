using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_IODevices : Entity
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public virtual ICollection<RCS_IOSignals> Signals { get; set; }



        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
