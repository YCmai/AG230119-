using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WMS.MaterialModule.Domain;
using System.Linq;

namespace WMS.MaterialModule
{
    [Route("/api/wms/material/statistic")]
    public class MaterialStatisticAppService : ApplicationService
    {
        IRepository<MaterialStatistics> _statisticRepository;
        IRepository<MaterialInfo> _materialRepository;
        public MaterialStatisticAppService(IRepository<MaterialStatistics> statisticRepository, IRepository<MaterialInfo> materialRepository)
        {
            _statisticRepository = statisticRepository;
            _materialRepository = materialRepository;
        }
        [HttpGet]
        public async Task<PagedResultDto<MaterialStatisticDto>> Statistic(MaterialStatisticInput input)
        {
            var statistics = (await _statisticRepository.GetQueryableAsync())
                    .WhereIf(!string.IsNullOrEmpty(input.Sku), m => m.SKU.Contains(input.Sku))
                    .WhereIf(!string.IsNullOrEmpty(input.WareHouseId), m => m.WareHouseId.Contains(input.WareHouseId));
            var materials = await _materialRepository.GetQueryableAsync();
            var query = from statistic in statistics
                        join material in materials
                        on statistic.SKU equals material.Id
                        select new MaterialStatisticDto()
                        {
                            Sku = statistic.SKU,
                            AvailableQuatity = statistic.AvailableQuatity,
                            FreezeQuatity = statistic.FreezeQuatity,
                            LockedQuatity = statistic.LockedQuatity,
                            SumQuatity = statistic.SumQuatity,
                            WareHouseId = statistic.WareHouseId,
                            MaterialName = material.Name
                        };
            var count = query.Count();
            var items = query.OrderBy(m => m.Sku).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<MaterialStatisticDto>(count, items);
        }
    }
}
