using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{
    public class LineCallOutputOrderCreateInput
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string ReMark { get; set; }
        /// <summary>
        /// 创建者Id
        /// </summary>
        public Guid CreatorId { get; set; }
        /// <summary>
        /// 出库储位（呼叫时给出）
        /// </summary>
        public string OutputStorageId { get; set; }
        /// <summary>
        /// 需要出库的物料的SKU
        /// </summary>
        public string SKU { get;  set; }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WarehouseId { get; set; }

    }
}
