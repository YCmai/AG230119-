using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace WMS.MaterialModule.Domain
{

    public class MaterialStatistics : BasicAggregateRoot<Guid>
    {

        protected MaterialStatistics() { }

        internal MaterialStatistics(Guid Id, string SKU,int availableQuatity, int freezeQuatity, int lockedQuatity, string wareHouseId) : base(Id)
        {
            this.SKU = SKU;
            WareHouseId = wareHouseId;
            SetQuatity(availableQuatity, freezeQuatity, lockedQuatity);
        }

        /// <summary>
        /// 所属仓库
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }
        /// <summary>
        /// SKU
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string SKU { get; private set; } = "";  //todo，这个需要是唯一的。
        /// <summary>
        /// 总的统计数量（可用数量+冻结数量+已锁定数量）
        /// </summary>
        public int SumQuatity { get; private set; }
        /// <summary>
        /// 可用数量
        /// </summary>
        public int AvailableQuatity { get; private set; }
        /// <summary>
        /// 已冻结数量。
        /// </summary>
        public int FreezeQuatity { get; private set; }
        /// <summary>
        /// 已锁定数量。
        /// </summary>
        public int LockedQuatity { get; private set; }
        /// <summary>
        /// 设置数量
        /// </summary>
        /// <param name="AvailableQuatity"></param>
        /// <param name="FreezeQuatity"></param>
        /// <param name="LockedQuatity"></param>
        public void SetQuatity(int AvailableQuatity, int FreezeQuatity, int LockedQuatity)
        {
            if (AvailableQuatity < 0 || FreezeQuatity < 0 || LockedQuatity < 0) throw new Exception("值不能小于0");

            this.AvailableQuatity = AvailableQuatity;
            this.FreezeQuatity = FreezeQuatity;
            this.LockedQuatity = LockedQuatity;

            this.SumQuatity = AvailableQuatity + FreezeQuatity + LockedQuatity;
        }

    }
}
