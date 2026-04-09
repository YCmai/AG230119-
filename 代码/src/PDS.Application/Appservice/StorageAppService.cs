using Microsoft.AspNetCore.Mvc;
using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace PDS.Appservice
{
    [Route("PDS/api/Storage")]
    public class StorageAppService : PDSAppService
    {
        IRepository<CageCarStorage> _storageRepository;
        public StorageAppService(IRepository<CageCarStorage> storageRepository)
        {
            _storageRepository = storageRepository;
        }
        [HttpGet("Pagination")]
        public async Task<PagedResultDto<CageCarStorageDto>> GetPagination(GetStoragePaginationRequest request)
        {
            var query = (await _storageRepository.GetQueryableAsync()).WhereIf(!string.IsNullOrEmpty(request.Code),m=>m.MapNodeName.Contains(request.Code));
            var totalCount = query.Count();
            var items = query
                .OrderBy(m => m.Id)
                .Skip(request.SkipCount)
                .Take(request.MaxResultCount)
                .ToList();

            return new PagedResultDto<CageCarStorageDto>()
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CageCarStorage>, List<CageCarStorageDto>>(items)
            };
        }
        [HttpPost("Create")]
        public async Task<CageCarStorageDto> CreateAsync(CageCarStorageCreateInput input)
        {
                var entity = new CageCarStorage(input.MapNodeName, input.StorageType);
                await _storageRepository.InsertAsync(entity);
                return ObjectMapper.Map<CageCarStorage, CageCarStorageDto>(entity);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<CageCarStorageDto> DeleteAsync(string Id)
        {
            var entity = await _storageRepository.GetAsync(m => m.Id == Id);
            if (entity != null)
                await _storageRepository.DeleteAsync(m => m.Id == Id);
            return ObjectMapper.Map<CageCarStorage, CageCarStorageDto>(entity);
        }
    }
}
