
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.Localization;
using Volo.Abp.Settings;

namespace PDS.Appservice
{
    [Route("PDS/api/Setting")]
    public class SettingAppService : PDSAppService
    {
        ISettingManager _settingManager;
        ISettingDefinitionManager _definitionManager;
        public SettingAppService(ISettingManager settingManager, ISettingDefinitionManager settingDefinitionManager)
        {
            _settingManager = settingManager;

            _definitionManager = settingDefinitionManager;
            LocalizationResource = typeof(AbpSettingManagementResource);
        }
        /// <summary>
        /// 获取当前租户设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTenantValue")]
        public async Task<PagedResultDto<SettingDefinitionDto>> GetCurrentTenantSettingValues(GetCurrentTenantInput input)
        {
            var values = await _settingManager.GetAllForCurrentTenantAsync();
            var definitions = _definitionManager.GetAll();
            var dtos = from definition in definitions
                       join value in values
                       on definition.Name equals value.Name
                       into temp
                       from dv in temp.DefaultIfEmpty()
                       select new SettingDefinitionDto()
                       {
                           Name = definition.Name,
                           Description = definition.Description?.Localize(StringLocalizerFactory).Value,
                           DisplayName = definition.DisplayName?.Localize(StringLocalizerFactory).Value,
                           CurrentValue = dv?.Value
                       };
            var result = dtos.OrderBy(m => m.Name).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<SettingDefinitionDto>()
            {
                Items = result,
                TotalCount = dtos.Count()
            };
        }
        [HttpGet("GetGlobalValue")]
        public async Task<PagedResultDto<SettingDefinitionDto>> GetGlobalSettingValues(GetDefaultSettingValueInput input)
        {
            var values = await _settingManager.GetAllGlobalAsync();
            var definitions = _definitionManager.GetAll();
            var dtos = from definition in definitions
                       join value in values
                       on definition.Name equals value.Name
                       into temp
                       from dv in temp.DefaultIfEmpty()
                       select new SettingDefinitionDto()
                       {
                           Name = definition.Name,
                           Description = definition.Description?.Localize(StringLocalizerFactory).Value,
                           DisplayName = definition.DisplayName?.Localize(StringLocalizerFactory).Value,
                           CurrentValue = dv?.Value
                       };
            //var dtos = from value in values
            //           join definition in definitions
            //           on value.Name equals definition.Name
            //           select
            //           new SettingDefinitionDto
            //           {
            //               Name = value.Name,
            //               Description = definition.Description?.Localize(StringLocalizerFactory).Value,
            //               DisplayName = definition.DisplayName?.Localize(StringLocalizerFactory).Value,
            //               CurrentValue = value?.Value
            //           };

            var result = dtos.OrderBy(m => m.Name).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<SettingDefinitionDto>()
            {
                Items = result,
                TotalCount = dtos.Count()
            };
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("SetTenantValue")]
        public async Task SetTenantSettingValue(SetDefaultSettingValueInput input)
        {
            await _settingManager.SetForCurrentTenantAsync(input.Name, input.Value);
        }

        [HttpPost("SetGlobalValue")]
        public async Task SetGlobalSettingValue(SetDefaultSettingValueInput input)
        {
            await _settingManager.SetGlobalAsync(input.Name, input.Value);
        }
    }
}
