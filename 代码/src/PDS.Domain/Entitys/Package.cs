using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{

    //TODO,目前是没有出库记录可以查到的。入库可以通过查DeliverProcessTask来查询。
    public class Package : BasicAggregateRoot<string>
    {
        protected Package() { }
        public Package(string id, string packaExtInfo, string packagesortId,string size,double weight):this(id,packaExtInfo,packagesortId)
        {
            this.PackageSize = size;
            this.PackageWeight = weight;
        }

        public Package(string id,string packaExtInfo, string packagesortId)
        {
            this.Id = id;
            this.PackaExtInfo = packaExtInfo;
            this.PackageSortId = packagesortId;
            this.CreationTime = DateTime.Now;
        }

        public virtual string PackaExtInfo { get; protected set; }
        /// <summary>
        /// 包裹所属分类
        /// </summary>
        public virtual string PackageSortId { get; protected set; }
        /// <summary>
        /// 长宽高，用"-"隔开（单位mm，例如 100-100-50）
        /// </summary>
        public virtual string? PackageSize { get; protected set; }
        /// <summary>
        /// 重量（kg）
        /// </summary>
        public virtual double? PackageWeight { get; protected set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreationTime { get; protected set; }
        
    }
}
