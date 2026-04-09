using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using WMS.MaterialModule.Domain;
using WMS.MaterialModule.Domain.Shared;

namespace WMS.MaterialModule.Application
{

    /// <summary>
    /// 捕获MaterialItem的更新，自动删除空记录。
    /// </summary>
    class MaterialItemUpdatedEventHandle : ILocalEventHandler<EntityUpdatedEventData<MaterialItem>>, ITransientDependency
    {
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;

        public MaterialItemUpdatedEventHandle(
            IRepository<MaterialItem, Guid> MaterialItemRepos)
        {
            _materialItemRepos = MaterialItemRepos;
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<MaterialItem> eventData)
        {
            if(eventData.Entity.AvailableQuatity==0 && eventData.Entity.FreezeQuatity==0 && eventData.Entity.LockedQuatity==0)
            {
                await _materialItemRepos.DeleteAsync(eventData.Entity);
            }
        }
    }

}
