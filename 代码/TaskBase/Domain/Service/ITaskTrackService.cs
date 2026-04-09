using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Domain.Shared;

namespace TaskBaseModule.Domain
{
	public interface ITaskTrackService
	{
		//Task AddAssociation<parentTaskHandleT>(Guid taskId, Guid parentId, Type parentTaskType);

		Task AddAssociation<parentTaskT, parentTaskHandleT>(Guid subTaskId, parentTaskT parent) where parentTaskT : TaskBaseAggregateRoot where parentTaskHandleT : IParentTaskHandle<parentTaskT>;
		Task AddAssociation<parentTaskT, parentTaskHandleT>(TaskBaseAggregateRoot subTask, parentTaskT parent) where parentTaskT : TaskBaseAggregateRoot where parentTaskHandleT : IParentTaskHandle<parentTaskT>;
		//这个方法已经实现，但使用的时候比较容易写错，屏蔽掉不用。
		//Task AddAssociation(TaskBaseAggregateRoot subTask, TaskBaseAggregateRoot parent);
	}
}
