using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TaskBaseModule.Domain.Shared
{
    ///// <summary>
    ///// 开始任务
    ///// </summary>
    ///// <typeparam name="TTask"></typeparam>
    //public interface ITaskHandle<TTask>:ITaskHandle, ITransientDependency where TTask:TaskBaseAggregateRoot
    //{
    //    //Task StartTask(TTask task);
    //}

    /// <summary>
    /// 当有子任务更新时，调度此函数。
    /// </summary>
    public interface IParentTaskHandle
    {
        Task UpdateTaskWhenSubtaskUpdatingAsync(TaskBaseAggregateRoot basicTask);
    }
}
