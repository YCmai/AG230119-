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

namespace WMS.MaterialModule.Application
{
    [Route("api/WMS/MaterialRecord")]
    public class MaterialRecordAppService : ApplicationService
    {
        private readonly IRepository<MaterialRecord, Guid> _materialRecordRepos;
        private readonly IRepository<MaterialInfo, string> _materialInfoRepos;
        private readonly MaterialManager _materialManager;

        public MaterialRecordAppService(
            IRepository<MaterialRecord, Guid> MaterialRecordRepos,
            IRepository<MaterialInfo, string> MaterialInfoRepos,
            MaterialManager materialManager
            )
        {
            _materialRecordRepos = MaterialRecordRepos;
            _materialInfoRepos = MaterialInfoRepos;
            _materialManager = materialManager;
        }

        [HttpGet("GetList")]
        public async Task<PagedResultDto<MaterialRecordDto>> GetList(GetAllMaterialRecordPageInput input)
        {
            var records = (await this._materialRecordRepos.GetQueryableAsync())
                .WhereIf(!input.Sku.IsNullOrWhiteSpace(), p => p.MaterialInfoId.Contains(input.Sku))
                .WhereIf(!input.StorageId.IsNullOrWhiteSpace(), p => p.StorageId.Contains(input.StorageId))
                .WhereIf(!input.WhereHouseId.IsNullOrWhiteSpace(), p => p.WareHouseId.Contains(input.WhereHouseId))
                .WhereIf(!input.Barcode.IsNullOrWhiteSpace(), p => p.BarCode.Contains(input.Barcode))
                .OrderBy(p => p.StoreTime);

            var query = from record in records
                        join info in (await _materialInfoRepos.GetQueryableAsync())
                        on record.MaterialInfoId equals info.Id
                        into tempInfos
                        from tempInfo in tempInfos.DefaultIfEmpty()
                        where tempInfo.Id != null
                        select new MaterialRecordDto()
                        {
                            BarCode = record.BarCode,
                            Batch = record.Batch,
                            Id = record.Id,
                            MaterialInfoId = record.MaterialInfoId,
                            OutputTime = record.OutputTime,
                            Quatity = record.Quatity,
                            StorageId = record.StorageId,
                            StoreTime = record.StoreTime,
                            WareHouseId = record.WareHouseId,

                            InContainer = tempInfo.IsContainer,
                            SizeMess = tempInfo.SizeMess,
                            Category = tempInfo.Category,
                            Describtion = tempInfo.Describtion,
                            Name = tempInfo.Name,
                        };

            var total = query.Count();
            var entities = query.OrderBy(m => m.StoreTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<MaterialRecordDto>(total, entities);
        }
    }

}
