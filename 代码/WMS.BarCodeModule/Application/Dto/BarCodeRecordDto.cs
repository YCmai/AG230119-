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

namespace WMS.BarCodeModule.Application.Dto
{

    /// <summary>
    /// 条码记录
    /// </summary>
    public class BarCodeRecordDto : AggregateRoot<Guid>
    {
        /// <summary>
        /// 父条码
        /// </summary>
        public string FatherBarcode { get; set; }
        /// <summary>
        /// 父条码对应的数量（拆分前）
        /// </summary>
        public int FatherQuantity { get; set; }
        /// <summary>
        /// 拆分出来的子条码
        /// </summary>
        public string ChildBarcode { get; set; }
        /// <summary>
        /// 子条码
        /// </summary>
        public int ChildQuantity { get; set; }
        /// <summary>
        /// 拆分时间
        /// </summary>
        public DateTime Time { get; set; }
    }
}
