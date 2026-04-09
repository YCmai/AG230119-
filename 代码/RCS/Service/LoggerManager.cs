using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using WMS.StorageModule.Domain.Shared;

using WMS.StorageModule.Domain;

using Volo.Abp.DependencyInjection;
using Volo.Abp;
using WMS.MaterialModule.Domain;
using TaskBaseModule.Domain.Shared;
using AciModule.Domain.Entitys;
using Volo.Abp.Guids;
using Microsoft.Extensions.Logging;

namespace WMS.LineCallInputModule.Domain
{

    /// <summary>
    /// 实现对LineCallInputOrder的启动移储。
    /// </summary>
    public class LoggerManager : DomainService
    {
        private readonly ILogger<LoggerManager> _logger;
        private readonly IRepository<RCS_TaskErrRecord> _rcs_TaskErrRecord;

        public LoggerManager(
            ILogger<LoggerManager> logger, IRepository<RCS_TaskErrRecord> rcs_TaskErrRecord
           )
        {
            _logger = logger;
            _rcs_TaskErrRecord = rcs_TaskErrRecord;
        }

        [UnitOfWork]
        public async Task LogAndLogCritical(string message)
        {
            _logger.LogCritical(message);
            //await _rcs_TaskErrRecord.InsertAsync(new RCS_TaskErrRecord() { CreataTime = DateTime.Now, TaskRemark = message, TaskType = 0 });
        }

        [UnitOfWork]
        public async Task LogAndLogError(string message)
        {
            _logger.LogError(message);
            //await _rcs_TaskErrRecord.InsertAsync(new RCS_TaskErrRecord() { CreataTime = DateTime.Now, TaskRemark = message, TaskType = 1 });

        }

      
    }
}
