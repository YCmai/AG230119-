using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using TaskBaseModule.Domain;
using Microsoft.Extensions.DependencyInjection;
using TaskBaseModule.Domain.Shared;

namespace PDS
{
    /// <summary>
    /// 当基础任务有变更时，通过事件触对父对象的更新。
    /// </summary>
    public class TaskUpdatingHandle : ILocalEventHandler<EntityUpdatedEventData<TaskBaseAggregateRoot>>, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<TaskTrackInfo> _taskTrackRepos;

        public TaskUpdatingHandle(IServiceProvider serviceProvider, IRepository<TaskTrackInfo> taskTrackRepos)
        {
            _serviceProvider = serviceProvider;
            _taskTrackRepos = taskTrackRepos;
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<TaskBaseAggregateRoot> eventData)
        {
            //获取信息
            if (!eventData.Entity.ParentId.HasValue) return;
			var taskTrack = await _taskTrackRepos.FindAsync(p => p.ParentTaskId == eventData.Entity.ParentId && p.TaskId == eventData.Entity.Id);

            if (taskTrack == null) return;

            var temp = _serviceProvider.GetService(Type.GetType(taskTrack.ParentTaskHandleTypeFullName));

            //获取处理的实体  //todo,应该获取父对象的类型，接着通过反射，找出这个父对象的处理函数，再调用函数。目前还没有做反射的处理，暂时先这么用。
            var parentTaskHandle = temp as IParentTaskHandle;

            //执行处理
            await parentTaskHandle?.UpdateTaskWhenSubtaskUpdatingAsync(eventData.Entity);
        }
        //}}

    }

}
