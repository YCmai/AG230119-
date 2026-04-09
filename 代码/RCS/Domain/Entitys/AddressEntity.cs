using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Spark.Domain.Entitys
{
    public class AddressEntity : AggregateRoot<Guid>
    {
        protected AddressEntity()
        {

        }
        /// <summary>
        /// 请求名称标记
        /// </summary>
        public string RequestName { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// 携带token
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
