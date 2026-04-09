using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using WMS.LineCallInputModule.Domain;
using WMS.StorageModule.Domain;
using WMS.StorageModule.Domain.Shared;
using WMS.MaterialModule.Domain;
using Volo.Abp.DependencyInjection;

namespace WMS.LineCallInputModule.Domain
{
    [ExposeServices(typeof(IOutputStoragePolicy))]
    public class DefaultOutputStoragePolicy : IOutputStoragePolicy,ITransientDependency
    {
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly IRepository<MaterialItem, Guid> _materialRepos;
        private readonly IRepository<WareHouse, string> _wareHouseRepos;

        public DefaultOutputStoragePolicy(
            IRepository<Storage,string> storageRepos,
            IRepository<MaterialItem,Guid> materialRepos,
            IRepository<WareHouse,string> wareHouseRepos
            )
        {
            _storageRepos = storageRepos;
            _materialRepos = materialRepos;
            _wareHouseRepos = wareHouseRepos;
        }

        /// <summary>
        /// 选择一个物料作出库
        /// </summary>
        /// <param name="lineCallInputOrderDto"></param>
        /// <returns></returns>
        public async Task<MaterialItem> DecideMaterialToOutput(LineCallOutputOrder lineCallInputOrder)
        {
            //获取同一个仓库中的，存储区，任意一个有指定物料的储位。
            var TargetWareHouse = await _wareHouseRepos.FindAsync(lineCallInputOrder.WarehouseCode, includeDetails: true);
            if (TargetWareHouse == null) return null;
            WareHouseArea TargetArea = null;
            foreach(var area in TargetWareHouse.Areas)
            {
                if(area.AreaCategory== DefaultAreaCategory.LineCallStorageArea)
                {
                    TargetArea = area;
                    break;
                }
            }
            if(TargetArea == null) return null;

            var query = from m in (await _materialRepos.GetQueryableAsync()).Where(p => p.MaterialInfoId == lineCallInputOrder.SKU && p.AvailableQuatity >= 1)  //可用物料
                        join s in (await _storageRepos.GetQueryableAsync()).Where(p => p.WareHouseId == TargetWareHouse.Id && p.CurrentNodeMaterialCount > 0 && p.Locks.Count == 0) //在相同仓库，且储位没有被锁定
                        on m.StorageId equals s.Id
                        select m;

            return query.FirstOrDefault();
        }
    }
}
