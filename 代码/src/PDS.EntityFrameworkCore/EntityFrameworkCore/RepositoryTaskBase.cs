using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using PDS.Domain.Entitys;
using WMS.MaterialModule.Domain.Shared;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using System;

namespace PDS.EntityFrameworkCore
{

    //这个是自定义的仓储，为了额可以得到实体的类型。接着通过实体类型就可以使用ioc解析出对应的实体，根据Type名称就可以自动获取到仓储实体。
    public abstract class RepositoryTaskBase<TTask> : EfCoreRepository<AbpDbContext<PDSDbContext>, TTask, Guid>, IRepositoryTaskBase<TTask> where TTask : TaskBaseAggregateRoot
    {
        public Type TaskType => typeof(TTask);

        public RepositoryTaskBase(IDbContextProvider<AbpDbContext<PDSDbContext>> dbContextProvider)
        : base(dbContextProvider)
        {

        }
    }
}
