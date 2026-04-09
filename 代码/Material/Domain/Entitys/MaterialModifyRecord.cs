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
    /// 物流修改记录
    /// </summary>
    public class MaterialModifyRecord : BasicAggregateRoot<Guid>
    {

        protected MaterialModifyRecord() { }

        public MaterialModifyRecord(Guid id,string barCode, string sku,int oldCount,int newCount,DateTime time, Guid? userId):base(id)
        {
            if (oldCount == newCount) throw new Exception("更改前后的数量一致");
            this.BarCode = barCode;
            this.SKU = sku;
            this.OldCount= oldCount; ;
            this.NewCount = newCount;
            this.Time = time;
            this.UserId = userId;
            if (NewCount > oldCount) ModifyType = EModifyType.Gain;
            else ModifyType = EModifyType.Loss;
        }
        public string SKU { get; private set; }
        /// <summary>
        /// 更改前的数量
        /// </summary>
        public int OldCount { get; private set; }
        /// <summary>
        /// 更改后的数量
        /// </summary>
        public int NewCount { get; private set; }
        /// <summary>
        /// 更改的时间
        /// </summary>
        public DateTime Time { get;  private set; }
        /// <summary>
        /// 更改者Id
        /// </summary>
        public Guid? UserId { get; private set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string BarCode { get; private set; }
        /// <summary>
        /// 类型
        /// </summary>
        public EModifyType ModifyType { get; private set; }
     

    }
    public enum EModifyType:int
    {
        /// <summary>
        /// 赢
        /// </summary>
        Gain = 0,
        /// <summary>
        /// 亏
        /// </summary>
        Loss = 1,

    }
}
