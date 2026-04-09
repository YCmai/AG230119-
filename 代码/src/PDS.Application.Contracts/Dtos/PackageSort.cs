using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Application.Contracts.Dtos
{
    /// <summary>
    /// 包裹分类；根据分配来进行投递口选择；
    /// </summary>
    public class PackageSortDto : AggregateRoot<string>
    {
        public string Describe { get; protected set; }
    }
}



