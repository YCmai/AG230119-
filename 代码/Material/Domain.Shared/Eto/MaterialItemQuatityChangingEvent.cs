using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule.Domain.Shared
{
    /// <summary>
    /// 实体状态修改事件。对象的值已经改变，但还没有调用数据库的保存。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TState"></typeparam>
    public class MaterialItemQuatityChangingEvent
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity">被修改物料明细的对象</param>
        /// <param name="oldAvailableQuatity"></param>
        /// <param name="oldFreezeQuatity"></param>
        /// <param name="oldLockedQuatity"></param>
        public MaterialItemQuatityChangingEvent(MaterialItem entity, int oldAvailableQuatity, int oldFreezeQuatity, int oldLockedQuatity)
        {
            this.Entity = entity;
            this.OldAvailableQuatity = oldAvailableQuatity;
            this.OldFreezeQuatity = oldFreezeQuatity;
            this.OldLockedQuatity = oldLockedQuatity;
        }


        /// <summary>
        /// 值已经修改过后的实体
        /// </summary>
        public MaterialItem Entity { get;private set; }
        /// <summary>
        /// 修改前的可用数量
        /// </summary>
        public int OldAvailableQuatity { get;private set; }
        /// <summary>
        /// 修改前的冻结数量
        /// </summary>
        public int OldFreezeQuatity { get;private set; }
        /// <summary>
        /// 修改前的锁定数量
        /// </summary>
        public int OldLockedQuatity { get;private set; }

    }
}
