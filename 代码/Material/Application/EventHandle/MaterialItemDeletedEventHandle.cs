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
    /// 捕获MaterialItem的更新，自动同步到统计表
    /// </summary>
    class MaterialItemDeletedEventHandle : ILocalEventHandler<EntityDeletedEventData<MaterialItem>>, ITransientDependency
    {
        private readonly IRepository<MaterialStatistics, Guid> _materialStatisticsReps;
        private readonly IRepository<StorageModule.Domain.Storage, string> _storageRepos;

        public MaterialItemDeletedEventHandle(
            IRepository<MaterialStatistics, Guid> materialStatisticsReps,
            IRepository<StorageModule.Domain.Storage,string> storageRepos
            )
        {
            this._materialStatisticsReps = materialStatisticsReps;
            _storageRepos = storageRepos;
        }

        public async Task HandleEventAsync(EntityDeletedEventData<MaterialItem> eventData)
        {
            var AvailableQuatityChange = -eventData.Entity.AvailableQuatity;
            var FreezeQuatityChange = -eventData.Entity.FreezeQuatity;
            var LockQuatityChange = -eventData.Entity.LockedQuatity;

            if (AvailableQuatityChange == 0 && FreezeQuatityChange == 0 && LockQuatityChange == 0) return;

            var Statistics = await this._materialStatisticsReps.FindAsync(p => p.SKU == eventData.Entity.MaterialInfoId);
            if (Statistics == null)
            {
                throw new Exception("尝试修改物料库存时发现没有对应统计数据");
            }
            else
            {
                Statistics.SetQuatity(
                    Statistics.AvailableQuatity + AvailableQuatityChange,
                    Statistics.FreezeQuatity + FreezeQuatityChange,
                    Statistics.LockedQuatity + LockQuatityChange);
                await this._materialStatisticsReps.UpdateAsync(Statistics);
            }



            //更改储位对物料数量的统计
            var storage = await _storageRepos.GetAsync(eventData.Entity.StorageId);
            storage.SetMaterialCount(storage.CurrentNodeMaterialCount + AvailableQuatityChange + FreezeQuatityChange + LockQuatityChange);
            await _storageRepos.UpdateAsync(storage);
        }
    }
}
