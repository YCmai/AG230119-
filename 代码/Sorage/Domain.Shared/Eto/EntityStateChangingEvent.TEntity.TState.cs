using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule.Domain.Shared
{
    /// <summary>
    /// 实体状态修改事件。对象的值已经改变，但还没有调用数据库的保存。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TState"></typeparam>
    public class EntityStateChangingEvent<TEntity, TState>
    {
        public TEntity Entity { get; private set; }
        public TState OldState { get; private set; }
        public TState NewState { get; private set; }
        public EntityStateChangingEvent(TEntity entity, TState OldState, TState NewState)
        {
            this.Entity = entity;
            this.OldState = OldState;
            this.NewState = NewState;
        }
    }
}
