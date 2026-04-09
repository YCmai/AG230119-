using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Spark.Domain.Entitys
{
    public class TransformEntity:AggregateRoot<Guid>
    {
        protected TransformEntity()
        {

        }
        /// <summary>
        /// 起始位置
        /// </summary>
        public string FeedArea { get; protected set; }
        /// <summary>
        /// 取货站点
        /// </summary>
        public int PickupSite { get; protected set; }
        /// <summary>
        /// 取货高度
        /// </summary>
        public int PickupHeight { get; protected set; }
        /// <summary>
        /// 卸货具体位置
        /// </summary>
        public  string DischargeArea { get; protected set; }
        /// <summary>
        /// 卸货站点
        /// </summary>
        public int UnloadSite { get; protected set; }
        /// <summary>
        /// 卸货高度
        /// </summary>
        public int UnloadHeight { get; protected set; }

    }
}
