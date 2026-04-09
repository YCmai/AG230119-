using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace WMS.StorageModule.Domain.Shared
{
    public interface IStorageDbContext : IEfCoreDbContext
    {
        DbSet<Storage> Storages { get; }
        DbSet<WareHouse> WareHouses { get; set; }
    }
}
