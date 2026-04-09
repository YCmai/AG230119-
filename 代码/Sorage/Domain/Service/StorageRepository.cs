using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using WMS.StorageModule.Domain.Shared;

namespace WMS.StorageModule.Domain
{

    //public class StorageRepository : EfCoreRepository<IStorageDbContext, Storage, Guid>, IStorageRepository
    //{
    //    public StorageRepository(IDbContextProvider<IStorageDbContext> dbContextProvider) : base(dbContextProvider)
    //    {
    //    }

    //    public Task<IQueryable<Storage>> GetIncludeChildrenAsync(Guid storageId, bool includeDetails = false, CancellationToken cancellationToken = default)
    //    {
            
    //    }
    //}
}
