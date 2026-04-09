using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using WMS.MaterialModule.Domain.Shared;

namespace WMS.MaterialModule.Domain
{

    /// <summary>
    /// 物料信息（id就是对应的sku）
    /// </summary>
    public class MaterialInfo : AggregateRoot<string>
    {

        protected MaterialInfo() { }

        internal MaterialInfo(string sku, string name, bool isContainer, string category, string Describtion, string sizeMess) : base(sku)
        {
            this.Name = name;
            this.Describtion = Describtion;
            this.IsContainer = isContainer;
            this.Category = category;
            this.SizeMess = sizeMess;
        }
        /// <summary>
        /// 是否为容器
        /// </summary>
        public bool IsContainer { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; }
        /// <summary>
        /// 物料类别(留用）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Category { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string Describtion { get; set; }
        /// <summary>
        /// 尺寸信息
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string SizeMess { get; set; }

    }
}
