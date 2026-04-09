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
    [ExposeServices(typeof(IInputStoragePolicy))]
    public class DefaultInputStoragePolicy : IInputStoragePolicy,ITransientDependency
    {
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly IRepository<MaterialItem, Guid> _materialRepos;
        private readonly IRepository<WareHouse, string> _wareHouseRepos;

        public DefaultInputStoragePolicy(
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
        /// 选择一个储位作为搬运的存储储位
        /// </summary>
        /// <param name="lineCallInputOrderDto"></param>
        /// <returns></returns>
        public async Task<Storage> DecideStorageToDeliver(LineCallInputOrder lineCallInputOrder)
        {
            //获取同一个仓库中的，下料区的，任意一个空闲的储位。

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
            return await _storageRepos.FirstOrDefaultAsync(p =>
            p.WareHouseId == TargetWareHouse.Id //相同仓库
            && p.WareHouseIdAreaCode == TargetArea.Code //在线边存储区
            && p.CurrentNodeMaterialCount == 0 //没有物料
            && p.Locks.Count == 0); //没有被锁定
            //);
        }
    }
}
