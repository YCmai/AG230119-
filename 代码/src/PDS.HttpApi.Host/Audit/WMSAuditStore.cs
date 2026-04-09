using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;

namespace WMS
{
    public class WMSAuditStore : IAuditingStore, ISingletonDependency
    {
        ILogger<WMSAuditStore> _logger;
        public WMSAuditStore(ILogger<WMSAuditStore> logger)
        {
            _logger = logger;
        }
        public Task SaveAsync(AuditLogInfo auditInfo)
        {
            _logger.LogInformation(auditInfo.ToString());
            return Task.CompletedTask;
        }
    }
}
