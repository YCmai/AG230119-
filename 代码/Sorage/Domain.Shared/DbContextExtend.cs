using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.StorageModule.Domain;

namespace WMS.StorageModule.Domain.Shared
{
    public static class DbContextExtend
    {
        public static void ConfigureStorageModule(this ModelBuilder builder)
        {
            builder.Entity<StorageLock>(b =>
            {
                b.HasOne<Storage>().WithMany(p => p.Locks).HasForeignKey(o => o.StorageId);  
                b.HasKey(t => new { t.StorageId, t.LockerId });
            });

            builder.Entity<Storage>().Property(p => p.Id).HasMaxLength(64);



            builder.Entity<WareHouseArea>(b =>
            {
                b.HasOne<WareHouse>().WithMany(p => p.Areas).HasForeignKey(o => o.WareHouseId);
                b.HasKey(t => new { t.WareHouseId, t.Code });
            });

            builder.Entity<WareHouse>().Property(p=> p.Id).HasMaxLength(64);

        }
    }
}
