using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WMS.MaterialModule.Domain;

namespace WMS.MaterialModule
{
    [Route("/api/wms/material-info")]
    public class MaterialInfoAppService : ApplicationService
    {
        IRepository<MaterialInfo> _materialRepository;
        public MaterialInfoAppService(IRepository<MaterialInfo> materialRepository)
        {
            _materialRepository = materialRepository;
        }
        [HttpGet("page")]
        public async Task<PagedResultDto<MaterialInfoDto>> GetPageAsync(GetPageMaterialInfoInput input)
        {
            var query = (await _materialRepository.GetQueryableAsync());
            var count = query.Count();
            var items = query
                .OrderBy(m => m.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            return new PagedResultDto<MaterialInfoDto>(count, ObjectMapper.Map<List<MaterialInfo>, List<MaterialInfoDto>>(items));
        }
        [HttpGet("{sku}")]
        public async Task<MaterialInfoDto> GetAsync(string sku)
        {
            var material = await _materialRepository.GetAsync(m => m.Id == sku);
            return ObjectMapper.Map<MaterialInfo, MaterialInfoDto>(material);
        }
        [HttpPost("create")]
        public async Task<MaterialInfoDto> CreateAsync(CreateMaterialInfoInput input)
        {
            var material = await _materialRepository.InsertAsync(new MaterialInfo(input.SKU, input.Name, input.IsContainer, input.Category, input.Describtion, input.SizeMess));
            return ObjectMapper.Map<MaterialInfo, MaterialInfoDto>(material);
        }
        [HttpPut("{sku}")]
        public async Task<MaterialInfoDto> UpdateAsync(string sku, UpdateMaterialInfoInput input)
        {
            var material = await _materialRepository.GetAsync(m => m.Id == sku);
            material.Name = input.Name;
            material.SizeMess = input.SizeMess;
            material.Describtion = input.Describtion;
            input.IsContainer = input.IsContainer;
            return ObjectMapper.Map<MaterialInfo, MaterialInfoDto>(material);
        }
        [HttpDelete("{sku}")]
        public async Task<MaterialInfoDto> DeleteAsync(string sku)
        {
            var material = await _materialRepository.GetAsync(m => m.Id == sku);
            await _materialRepository.DeleteAsync(material);
            return ObjectMapper.Map<MaterialInfo, MaterialInfoDto>(material);
        }
    }
}
