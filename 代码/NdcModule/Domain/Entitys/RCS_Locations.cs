using AciModule.Domain.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace AciModule.Domain.Entitys
{
    public class RCS_Locations : Entity
    {
        public int Id { get; set; }

       
        public string? Name { get; set; } //库位名字，唯一

       
        public string? NodeRemark { get; set; }

        public string? MaterialCode { get; set; } //物料编码，允许为空

        public string? PalletID { get; set; } = "0"; //托盘ID

        public string? Weight { get; set; } = "0"; //重量，允许为空

        public string? Quanitity { get; set; } //数量，允许为空

        public string? EntryDate { get; set; }



        public string? Group { get; set; } //分组

        public string? LiftingHeight { get; set; }//举升高度

      
        public bool Lock { get; set; }//是否锁定

        /// <summary>
        /// 等待点
        /// </summary>
        public string? WattingNode { get; set; }




        public override object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
