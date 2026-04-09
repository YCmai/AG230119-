using Microsoft.AspNetCore.Mvc;
using PDS.Application.Contracts.Dtos;
using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace PDS.Appservice
{
    [Route("/PDS/api/Match")]
    public class PackageRegularFormatMatchAppService: PDSAppService
    {
        IRepository<PackageSort> _sortRepository;
        IRepository<PackageRegularFormat> _formatRepository;
        public PackageRegularFormatMatchAppService(IRepository<PackageRegularFormat> formatRepository, IRepository<PackageSort> sortRepository)
        {
            _sortRepository = sortRepository;
            _formatRepository = formatRepository;
        }
        [HttpPost("Create")]
        public async Task<CommonResponseDto> CreateAsync([FromBody]PackageRegularFormatCreate input)
        {
            var sort = await _sortRepository.FirstOrDefaultAsync(m => m.Id == input.PackageSortId);
            if (sort == null)
                throw new AbpException("请选择一个有效的分类");
            var entity = ObjectMapper.Map<PackageRegularFormatCreate, PackageRegularFormat>(input);
            await _formatRepository.InsertAsync(entity);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }
        [HttpPost("Update")]
        public async Task<CommonResponseDto> UpdateAsync(PackageRegularFormatUpdate input)
        {
            var entity= await _formatRepository.GetAsync(m => m.Id == input.Id);
            if(entity==null) throw new AbpException("数据不存在!");
            ObjectMapper.Map(input, entity);
            await _formatRepository.UpdateAsync(entity);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }
        [HttpDelete("Delete")]
        public async Task<CommonResponseDto> DeleteAsync(string id)
        {
            await _formatRepository.DeleteAsync(m=>m.Id== id);
            return CommonResponseDto.CreateSuccessResponse(id);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetList")]
        public async Task<PagedResultDto<PackageRegularFormatDto>> GetListAsync(PackageRegularPageRequest input)
        {
            var totalCount = await _formatRepository.GetCountAsync();
            var items = await _formatRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, "RegularName", true);
            return new PagedResultDto<PackageRegularFormatDto>()
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<PackageRegularFormat>, List<PackageRegularFormatDto>>(items)
            };
        }
    }
}
