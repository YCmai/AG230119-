using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace PDS
{
    /// <summary>
    /// 正则表达式分类
    /// </summary>
    [Table("PackageRegularFormats")]
    public class PackageRegularFormat : Entity<string>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string RegularName { get; set; }
        /// <summary>
        /// 正则表达式
        /// </summary>
        public string RegularFormat { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>
        public string PackageSortId { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [ForeignKey("PackageSortId")]
        public PackageSort PackageSort { get; set; }

        [DisableAuditing]
        [Timestamp]
        public virtual string ConcurrencyStamp { get; set; }
    }
}
