using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WMS.StorageModule.Domain;

namespace WMS.StorageModule
{
    public class WareHouseManager : DomainService
    {
        IRepository<WareHouse> _wareHouseRepository;
        public WareHouseManager(IRepository<WareHouse> wareHouseRepository)
        {
            _wareHouseRepository = wareHouseRepository;
        }
        public async Task<WareHouse> CreateAsnyc(string id, string name, string description)
        {
            if (await CheckExitsAsync(id))
                throw new UserFriendlyException("库别编码不能重复!");
            return await _wareHouseRepository.InsertAsync(new WareHouse(id, name, description));
        }
        public async Task<bool> CheckExitsAsync(string id)
        {
            var exists = await _wareHouseRepository.FirstOrDefaultAsync(m => m.Id == id);
            return exists != null;
        }
    }
}
