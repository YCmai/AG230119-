using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace TaskBaseModule.Domain.Shared
{
    public abstract class TaskBaseAggregateRoot : AggregateRoot<Guid>
    {
        public TaskBaseAggregateRoot(Guid Id, Guid? parentTaskId, string parentTaskType) : base(Id)
        {
            this.ParentId = parentTaskId;
            this.ParentTaskTypeFullName = parentTaskType;
            this.CreationTime = DateTime.Now;
        }
        protected TaskBaseAggregateRoot() { }
        /// <summary>
        /// 父任务的Id
        /// </summary>
        public Guid? ParentId { get; private set; }
        /// <summary>
        /// 留用，以后可能通过这个属性来触发父任务的更新，而不是像现在一样手动添加关联。
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string? ParentTaskTypeFullName { get; protected set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; protected set; }
        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? CloseTime { get; protected set; }

        //public virtual async Task UpdateProcessWhenBasicTaskUpdatingAsync(BasicTaskAggregateRoot subTask) { }  //因为这个往往需要使用另外的接口，所以不在实体中定义。

    }
}
