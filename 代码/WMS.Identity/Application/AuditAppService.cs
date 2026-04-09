using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;

namespace WMS.Identity
{
    [Route("/api/audit")]
    public class AuditAppService : ApplicationService
    {
        IAuditLogRepository _auditRepository;
        public AuditAppService(IAuditLogRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }
        [HttpGet("get")]
        [Authorize(WmsIdentityPermissions.Audit.Default)]
        public async Task<PagedResultDto<AuditDto>> GetPageAsync(GetAuditRequestDto input)
        {
            var total = await _auditRepository.GetCountAsync();
            var items = (await _auditRepository.GetQueryableAsync())
                .OrderByDescending(m => m.ExecutionTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            return new PagedResultDto<AuditDto>(total, ObjectMapper.Map<List<AuditLog>, List<AuditDto>>(items));
        }
    }
}
