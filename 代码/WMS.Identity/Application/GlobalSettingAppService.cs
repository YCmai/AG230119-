using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace WMS.Identity
{
    /// <summary>
    /// todo:这样是不符合规范
    /// </summary>
    [Route("/api/setting/global")]
    public class GlobalSettingAppService : ApplicationService
    {
        ISettingManager _settingManager;
        ISettingDefinitionManager _definitionManager;
        public GlobalSettingAppService(ISettingManager settingManager, ISettingDefinitionManager definitionManager)
        {
            _settingManager = settingManager;
            _definitionManager = definitionManager;
        }
        [HttpGet("pagination")]
        [Authorize(WmsIdentityPermissions.Setting.Default)]
        public async Task<PagedResultDto<SettingDto>> GetPagination(SettingPageRequest input)
        {
            var values = await _settingManager.GetAllGlobalAsync();
            var definitions = _definitionManager.GetAll();
            var dtos = from definition in definitions
                       join value in values
                       on definition.Name equals value.Name
                       into temp
                       from dv in temp.DefaultIfEmpty()
                       select new SettingDto()
                       {
                           Name = definition.Name,
                           Description = definition.Description?.Localize(StringLocalizerFactory).Value,
                           DisplayName = definition.DisplayName?.Localize(StringLocalizerFactory).Value,
                           CurrentValue = dv?.Value
                       };

            var result = dtos.OrderBy(m => m.Name).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<SettingDto>()
            {
                Items = result,
                TotalCount = dtos.Count()
            };
        }
        [HttpPost("set")]
        [Authorize(WmsIdentityPermissions.Setting.Update)]
        public async Task SetValue(SettingValueUpdateDto input)
        {
            await _settingManager.SetGlobalAsync(input.Name, input.Value);
        }
    }
}
