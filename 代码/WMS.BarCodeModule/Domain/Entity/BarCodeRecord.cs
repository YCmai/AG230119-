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

namespace WMS.BarCodeModule.Domain
{

    /// <summary>
    /// 条码记录
    /// </summary>
    public class BarCodeRecord : AggregateRoot<Guid>
    {

        protected BarCodeRecord() { }

        public BarCodeRecord(string fatherBarcode,int fatherBarcodeQuantityBeforeSplit, string childBarcode,int childBarcodeQuantity)
        {
            this.FatherBarcode = fatherBarcode;
            this.FatherQuantity = fatherBarcodeQuantityBeforeSplit;
            this.ChildBarcode= childBarcode;
            this.ChildQuantity = childBarcodeQuantity;
        }

        /// <summary>
        /// 父条码
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string FatherBarcode { get; private set; }
        /// <summary>
        /// 父条码对应的数量（拆分前）
        /// </summary>
        public int FatherQuantity { get; private set; }
        /// <summary>
        /// 拆分出来的子条码
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string ChildBarcode { get; private set; }
        /// <summary>
        /// 子条码
        /// </summary>
        public int ChildQuantity { get; private set; }
        /// <summary>
        /// 拆分时间
        /// </summary>
        public DateTime Time { get; private set; }

        
    }
}
