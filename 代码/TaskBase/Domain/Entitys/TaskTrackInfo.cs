using System;
using Volo.Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBaseModule.Domain
{
    /// <summary>
    /// 任务跟踪 todo:这里使用复合key，有啥用么？
    /// </summary>
    public class TaskTrackInfo : BasicAggregateRoot
    {
        protected TaskTrackInfo() { }

        internal  TaskTrackInfo(Guid taskId, Guid parentTaskId,Type parentTaskType, Type parentTaskHandleType)
        {
            this.TaskId = taskId;
            this.ParentTaskId = parentTaskId;
            ParentTaskTypeFullName = parentTaskType.FullName;
            this.ParentTaskHandleTypeFullName = parentTaskHandleType.AssemblyQualifiedName;
        }

        public Guid TaskId { get; private set; }
        public Guid ParentTaskId { get; private set; }
        [Column(TypeName = "nvarchar(256)")]
        public string ParentTaskTypeFullName { get; private set; }
        [Column(TypeName = "nvarchar(256)")]
        public string ParentTaskHandleTypeFullName { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { TaskId, ParentTaskId };
        }
    }
}

