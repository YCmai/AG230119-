using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.LineCallProcessTaskModule.Domain;
using Volo.Abp.Application.Services;

namespace WMS.LineCallProcessTaskModule.Application
{
    [Route("api/WMS/LineCallInputTask")]
    public class LineCallInputAppservice : ApplicationService
    {
        private readonly IRepository<LineCallInputTask, Guid> _lineCallInputTaskRepos;

        public LineCallInputAppservice(
            IRepository<LineCallInputTask, Guid>  lineCallInputTaskRepos
            )
        {
            _lineCallInputTaskRepos = lineCallInputTaskRepos;
        }
        [HttpGet("GetList")]  
        public async Task<List<LineCallInputTaskDto>> GetList()
        {
            var entities = await this._lineCallInputTaskRepos.GetListAsync();
            return ObjectMapper.Map<List<LineCallInputTask>, List<LineCallInputTaskDto>>(entities);
        }

        //[HttpPost("Create")]
        //public async Task<LineCallInputTaskDto> CreateAsync([FromBody] LineCallInputTaskCreateInput lineCallInputTaskCreateInput)
        //{
        //    var entity = new LineCallInputTask(lineCallInputTaskCreateInput.TaskId,lineCallInputTaskCreateInput.LoadNodeName,lineCallInputTaskCreateInput.UnloadNodeName);
        //    entity = await this._lineCallInputTaskRepos.InsertAsync(entity);
        //    return ObjectMapper.Map<LineCallInputTask, LineCallInputTaskDto>(entity);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="storageDeleteInput"></param>
        ///// <returns></returns>
        //[HttpPost("Delete")]
        //public async Task DeleteAsync(Guid guid)
        //{
        //    await _lineCallInputTaskRepos.DeleteAsync(guid);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="storageDeleteInput"></param>
        ///// <returns></returns>
        //[HttpPost("DeleteAll")]
        //public async Task DeleteAllAsync()
        //{
        //    var all = await _lineCallInputTaskRepos.GetListAsync();
        //    await _lineCallInputTaskRepos.DeleteManyAsync(all);
        //}

    }
}



