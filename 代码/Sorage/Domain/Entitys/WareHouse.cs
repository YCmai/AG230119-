using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace WMS.StorageModule.Domain
{
    /// <summary>
    /// 仓库
    /// </summary>
    public class WareHouse : AggregateRoot<string>
    {
        protected WareHouse() { }

        public WareHouse(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 仓库
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; private set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string Description { get; private set; }
        /// <summary>
        /// 仓库区域
        /// </summary>
        public List<WareHouseArea> Areas { get; private set; } = new List<WareHouseArea>();


        public WareHouseArea AddArea(string areaCode, string areaName, string areaDescribetion, string areaCategory)
        {
            var oldArea = Areas.FirstOrDefault(m => m.Code == areaCode);
            if (oldArea != null)
                throw new UserFriendlyException("同一库别下区域编码不能重复");
            var NewArea = new WareHouseArea(areaCode, areaName, areaDescribetion, this.Id, areaCategory);
            Areas.Add(NewArea);
            return NewArea;
        }
    }

    /// <summary>
    /// 仓库区域
    /// </summary>
    public class WareHouseArea : Entity
    {
        protected WareHouseArea() { }

        internal WareHouseArea( string code, string name, string description, string wareHoseId,string areaCategory)
        {
            Code = code;
            Name = name;
            Description = description;
            WareHouseId = wareHoseId;
            AreaCategory = areaCategory;
        }
        /// <summary>
        /// 区域编码
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Code { get; private set; } 
        /// <summary>
        /// 区域名称
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; private set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string Description { get; private set; }
        /// <summary>
        /// 所属仓库
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }
        /// <summary>
        /// 区域类别（用来划分用途）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string AreaCategory { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { WareHouseId, Code };
        }
    }
}
