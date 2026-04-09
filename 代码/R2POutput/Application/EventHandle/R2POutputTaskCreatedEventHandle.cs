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
using WMS.R2POutputModule.Domain;

namespace WMS.R2POutputModule.Application
{
    /// <summary>
    /// 当成功添加流程后，马上开始处理。
    /// </summary>
    public class R2POutputTaskCreatedEventHandle : ILocalEventHandler<EntityCreatedEventData<R2POutputTask>>,ITransientDependency
    {
        private readonly R2POutputTaskTaskHandle _lineCallInputTaskHandle;

        public R2POutputTaskCreatedEventHandle(R2POutputTaskTaskHandle lineCallInputTaskHandle)
        {
            _lineCallInputTaskHandle = lineCallInputTaskHandle;
        }

        //流程创建时，同步添加“设备分配任务”，作为流程的第一步。
        public async Task HandleEventAsync(EntityCreatedEventData<R2POutputTask> eventData)  //todo,这里直接修改事件中对象，可行吗？是否会更新？
        {
            await _lineCallInputTaskHandle.StartTask(eventData.Entity);
        }
    }
}
