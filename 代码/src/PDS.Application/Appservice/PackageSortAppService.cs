using Microsoft.AspNetCore.Mvc;
using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace PDS
{
    [Route("PDS/api/PackageSort")]
    public class PackageSortAppService: PDSAppService
    {
        IRepository<PackageSort> _sortRepository;
        public PackageSortAppService(IRepository<PackageSort> sortRepository)
        {
            _sortRepository = sortRepository;
        }
        /// <summary>
        /// 获取格口分类
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public async Task<List<GetAllPackageSortDto>> GetAllAsync()
        {
            var items = await _sortRepository.GetListAsync();
            return ObjectMapper.Map<List<PackageSort>, List<GetAllPackageSortDto>>(items);
        }
    }
}
