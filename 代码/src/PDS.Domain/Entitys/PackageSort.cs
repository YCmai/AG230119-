using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    /// <summary>
    /// 包裹分类；根据分配来进行投递口选择；
    /// </summary>
    public class PackageSort : BasicAggregateRoot<string>
    {
        protected PackageSort() { }

        public PackageSort(string Id, string Describe)
        {
            this.Id = Id;
            this.Describe = Describe;
        }

        public string Describe { get; protected set; }

    }
}



