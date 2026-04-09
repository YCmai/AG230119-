using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Application.Dtos;
using System.Linq;
using WMS.BarCodeModule.Domain;
using WMS.BarCodeModule.Application.Dto;

namespace WMS.BarCodeModule.Application
{
    [Route("api/WMS/BarCode")]
    public class BarCodeAppservice : ApplicationService
    {
        private readonly IRepository<BarCodeRecord, Guid> _barCodeRecordRepos;

        public BarCodeAppservice(
            IRepository<BarCodeRecord, Guid> barCodeRecordRepos
            )
        {
            _barCodeRecordRepos = barCodeRecordRepos;
        }

        [HttpGet("GetList")]
        public async Task<PagedResultDto<BarCodeRecordDto>> GetList(GetBarCodePageInput input)
        {
            var query = (await this._barCodeRecordRepos.GetQueryableAsync())
                .WhereIf(!input.BarCode.IsNullOrWhiteSpace(), p => p.ChildBarcode == input.BarCode)
                .WhereIf(!input.ParentBarCode.IsNullOrWhiteSpace(), p => p.FatherBarcode == input.ParentBarCode);
            var total = query.Count();
            var entities = query.OrderBy(p => p.Time)
                                .Skip(input.SkipCount)
                                .Take(input.MaxResultCount)
                                .ToList();
            return new PagedResultDto<BarCodeRecordDto>(total, ObjectMapper.Map<List<BarCodeRecord>, List<BarCodeRecordDto>>(entities));
        }
    }

}
