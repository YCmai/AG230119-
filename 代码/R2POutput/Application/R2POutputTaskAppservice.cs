using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.R2POutputModule.Domain;
using Volo.Abp.Application.Services;

namespace WMS.R2POutputModule.Application
{
    [Route("api/WMS/LineCallInputTask")]
    public class R2POutputTaskAppservice : ApplicationService
    {
        private readonly IRepository<R2POutputTask, Guid> _lineCallInputTaskRepos;

        public R2POutputTaskAppservice(
            IRepository<R2POutputTask, Guid>  lineCallInputTaskRepos
            )
        {
            _lineCallInputTaskRepos = lineCallInputTaskRepos;
        }
        [HttpGet("GetList")]  
        public async Task<List<R2POutputTaskDto>> GetList()
        {
            var entities = await this._lineCallInputTaskRepos.GetListAsync();
            return ObjectMapper.Map<List<R2POutputTask>, List<R2POutputTaskDto>>(entities);
        }

        [HttpPost("Create")]
        public async Task<R2POutputTaskDto> CreateAsync([FromBody] R2POutputTaskCreateInput lineCallInputTaskCreateInput)
        {
            var entity = new R2POutputTask(lineCallInputTaskCreateInput.TaskId,lineCallInputTaskCreateInput.LoadNodeName,lineCallInputTaskCreateInput.UnloadNodeName);
            entity = await this._lineCallInputTaskRepos.InsertAsync(entity);
            return ObjectMapper.Map<R2POutputTask, R2POutputTaskDto>(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("Delete")]
        public async Task DeleteAsync(Guid guid)
        {
            await _lineCallInputTaskRepos.DeleteAsync(guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("DeleteAll")]
        public async Task DeleteAllAsync()
        {
            var all = await _lineCallInputTaskRepos.GetListAsync();
            await _lineCallInputTaskRepos.DeleteManyAsync(all);
        }

    }
}



