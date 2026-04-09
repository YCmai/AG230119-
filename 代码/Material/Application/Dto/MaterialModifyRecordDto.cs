using System;
using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Domain
{
    public class MaterialModifyRecordDto : EntityDto<Guid>
    {
        public MaterialModifyRecordDto()
        {

        }
        /// <summary>
        /// SKU
        /// </summary>
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
        public DateTime Time { get; private set; }
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
}
