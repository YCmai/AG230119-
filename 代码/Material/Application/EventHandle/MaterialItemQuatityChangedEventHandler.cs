using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using WMS.MaterialModule.Domain;
using WMS.MaterialModule.Domain.Shared;
using WMS.StorageModule.Domain;

namespace WMS.MaterialModule.Application
{

    /// <summary>
    /// 捕获MaterialItem的更新，自动同步到统计表
    /// </summary>
    class MaterialItemQuatityChangedEventHandler : ILocalEventHandler<MaterialItemQuatityChangingEvent>, ITransientDependency
    {
        private readonly IRepository<MaterialStatistics, Guid> _materialStatisticsReps;
        private readonly IRepository<Storage, string> _storageRepos;

        public MaterialItemQuatityChangedEventHandler(IRepository<MaterialStatistics, Guid> materialStatisticsReps,IRepository<Storage,string> storageRepos)
        {
            this._materialStatisticsReps = materialStatisticsReps;
            _storageRepos = storageRepos;
        }

        public async Task HandleEventAsync(MaterialItemQuatityChangingEvent eventData)
        {
            var AvailableQuatityChange = eventData.Entity.AvailableQuatity - eventData.OldAvailableQuatity;
            var FreezeQuatityChange = eventData.Entity.FreezeQuatity - eventData.OldFreezeQuatity;
            var LockQuatityChange = eventData.Entity.LockedQuatity - eventData.OldLockedQuatity;
            if (AvailableQuatityChange == 0 && FreezeQuatityChange == 0 && LockQuatityChange == 0) return;

            //更改物料统计
            var Statistics = await this._materialStatisticsReps.FindAsync(p => p.SKU == eventData.Entity.MaterialInfoId);
            if (Statistics == null)
            {
                await _materialStatisticsReps.InsertAsync(new MaterialStatistics(Guid.NewGuid(), eventData.Entity.MaterialInfoId, AvailableQuatityChange, FreezeQuatityChange, LockQuatityChange));
            }
            else
            {
                Statistics.SetQuatity(Statistics.AvailableQuatity + AvailableQuatityChange,
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
