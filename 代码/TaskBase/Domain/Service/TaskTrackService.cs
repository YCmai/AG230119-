using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Application;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TaskBaseModule.Domain.Service
{
    public class TaskTrackService : ITaskTrackService, ITransientDependency
    {
        IRepository<TaskTrackInfo> _TaskTrackInfoRepos;
		private readonly TaskHandleTypeMap _taskHandleTypeMap;

		public TaskTrackService(IRepository<TaskTrackInfo> taskTrackInfoRepos,TaskHandleTypeMap taskHandleTypeMap)
        {
            _TaskTrackInfoRepos = taskTrackInfoRepos;
			this._taskHandleTypeMap = taskHandleTypeMap;
		}

        //public async Task AddAssociation<parentTaskHandleT>(Guid taskId, Guid parentId, Type parentTaskType)
        //{
        //    await _TaskTrackInfoRepos.InsertAsync(new TaskTrackInfo(taskId, parentId, parentTaskType, typeof(parentTaskHandleT)));
        //}

        public async Task AddAssociation<T, parentTaskHandleT>(Guid taskId, T parent) 
            where T : TaskBaseAggregateRoot 
            where parentTaskHandleT : IParentTaskHandle<T>
        {
            await _TaskTrackInfoRepos.InsertAsync(new TaskTrackInfo(taskId, parent.Id, parent.GetType(), typeof(parentTaskHandleT)));
        }

		public async Task AddAssociation<parentTaskT, parentTaskHandleT>(TaskBaseAggregateRoot subTask, parentTaskT parent)
			where parentTaskT : TaskBaseAggregateRoot
			where parentTaskHandleT : IParentTaskHandle<parentTaskT>
		{
            await AddAssociation<parentTaskT, parentTaskHandleT>(subTask.Id, parent);
		}

		/// <summary>
		/// 通过反射添加，不过因为没有泛型的检查，无法约束parentTaskHandle与parentTask的关系。
		/// </summary>
		/// <param name="subTask"></param>
		/// <param name="parentTask"></param>
		/// <returns></returns>
		public async Task AddAssociation(TaskBaseAggregateRoot subTask,TaskBaseAggregateRoot parentTask)
        {
		    var parentTaskHandleType = _taskHandleTypeMap.GetTaskHandleType(parentTask.GetType());  //通过反射获取
			await _TaskTrackInfoRepos.InsertAsync(new TaskTrackInfo(subTask.Id, parentTask.Id, parentTask.GetType(), parentTaskHandleType));
		}
	}
}
