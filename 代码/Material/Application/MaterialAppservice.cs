using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.MaterialModule.Domain.Shared;
using WMS.MaterialModule.Domain;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Application.Dtos;
using System.Linq;
using Volo.Abp.Guids;
using WMS.StorageModule.Domain;
using Volo.Abp;

namespace WMS.MaterialModule.Application
{
    [Route("api/WMS/Material")]
    public class MaterialAppService : ApplicationService
    {
        private readonly IRepository<MaterialModifyRecord, Guid> _materialModifyRepos;
        private readonly IRepository<MaterialItem, Guid> _materialItemsReps;
        private readonly IRepository<MaterialInfo, string> _materialInfoRepos;
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly IGuidGenerator _guidGenerator;
        private readonly MaterialManager _materialManager;

        public MaterialAppService(
            IRepository<MaterialModifyRecord, Guid> materialModifyRepos,
            IRepository<MaterialItem, Guid> materialItemsReps,
            IRepository<MaterialInfo,string> MaterialInfoRepos, 
            IRepository<Storage,string> StorageRepos,
            IGuidGenerator guidGenerator,
            MaterialManager materialManager
            )
        {
            _materialModifyRepos = materialModifyRepos;
            _materialItemsReps = materialItemsReps;
            _materialInfoRepos = MaterialInfoRepos;
            _storageRepos = StorageRepos;
            _guidGenerator = guidGenerator;
            _materialManager = materialManager;
        }

        [HttpGet("GetList")]
        public async Task<PagedResultDto<MaterialItemDto>> GetList(GetAllMaterialPageInput input)
        {
            var items = (await this._materialItemsReps.GetQueryableAsync())
                .WhereIf(!input.Sku.IsNullOrWhiteSpace(), p => p.MaterialInfoId.Contains(input.Sku))
                .WhereIf(!input.StorageId.IsNullOrWhiteSpace(), p => p.StorageId.Contains(input.StorageId))
                .WhereIf(!input.WhereHouseId.IsNullOrWhiteSpace(), p => p.WareHouseId.Contains(input.WhereHouseId))
                .WhereIf(input.OnlyAvailable, p => p.AvailableQuatity > 0);

            var query = from item in items.OrderBy(p => p.StoreTime)
                         join info in (await _materialInfoRepos.GetQueryableAsync())
                         on item.MaterialInfoId equals info.Id
                         into tempItems
                         from tempItem in tempItems.DefaultIfEmpty()
                         where tempItem.Id!=null
                         select new MaterialItemDto()
                         {
                             AppData = "",
                             AvailableQuatity = item.AvailableQuatity,
                             Batch = item.Batch,
                             BarCode = item.BarCode,
                             FreezeQuatity = item.FreezeQuatity,
                             Id = item.Id,
                             LockedQuatity = item.LockedQuatity,
                             SKU = item.MaterialInfoId,
                             StorageId = item.StorageId,
                             StoreTime = item.StoreTime,
                             SumQuatity = item.SumQuatity,
                             WareHouseId = item.WareHouseId,

                             Describtion = tempItem.Describtion,
                             Category = tempItem.Category,
                             Name = tempItem.Name,
                             SizeMess = tempItem.SizeMess,
                             IsContainer = tempItem.IsContainer,
                         };


            var total = items.Count();
            var entities = query.OrderBy(m => m.SKU).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<MaterialItemDto>(total, entities);
        }


        /// <summary>
        /// 调整库存（增加）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("ModifyAdd")]
        public async Task<MaterialItemDto> ModifyAddAsync(AddMaterialInput input)
        {
            var materialInfo = await _materialInfoRepos.GetAsync(input.SKU);
            var storage = await _storageRepos.GetAsync(p=>p.Id == input.StorageId && p.WareHouseId== input.WareHouseId);
            var entity = await _materialManager.ModifyAdd(_guidGenerator.Create(), materialInfo, input.BarCode, input.Batch, input.Quatity, storage, CurrentUser.Id);
            return ObjectMapper.Map<MaterialItem, MaterialItemDto>(entity);
        }

        /// <summary>
        /// 调整库存（修改库存数量）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("ModifyUpdateAvailCount")]
        public async Task<MaterialItemDto> ModifyUpdateAvailCountAsync(MaterialAvailCountModifyInput input)
        {
            var materialItem = await _materialItemsReps.GetAsync(p => p.BarCode == input.BarCode && p.WareHouseId == input.WareHouseId);
            if (materialItem.AvailableQuatity != input.OldCount) throw new BusinessException("对象已更改，重新再试");
            if (materialItem.AvailableQuatity == input.NewCount) throw new BusinessException("无需更改");
            await _materialManager.ModifyUpdateAvailCount(materialItem, input.NewCount, CurrentUser.Id);
            return ObjectMapper.Map<MaterialItem, MaterialItemDto>(materialItem);
        }

        [HttpGet("GetModifyRecord")]
        public async Task<PagedResultDto<MaterialModifyRecordDto>> GetMaterialModifyRecord(GetAllMaterialModifyRecordInput input)
        {
            var query = (await _materialModifyRepos.GetQueryableAsync())
                .WhereIf(!input.barCode.IsNullOrWhiteSpace(), p => p.BarCode == input.barCode)
                .WhereIf(!input.sku.IsNullOrWhiteSpace(), p => p.SKU == input.sku)
                .OrderBy(p => p.Id)
                .OrderBy(p => p.SKU)
                .OrderBy(p => p.Time);

            var total = query.Count();
            var entities = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<MaterialModifyRecordDto>(total, ObjectMapper.Map<List<MaterialModifyRecord>, List<MaterialModifyRecordDto>>(entities));
        }
    }

}
