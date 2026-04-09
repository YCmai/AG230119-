using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.StorageModule.Domain;

namespace WMS.MaterialModule.Domain.Shared
{
    public static class DBContextExtend
    {
        public static void ConfigureMaterialModule(this ModelBuilder builder)
        {
            builder.Entity<MaterialStatistics>().HasIndex(p => new { p.WareHouseId, p.SKU }).IsUnique(); //唯一复合索引

            builder.Entity<MaterialItem>().HasIndex(p => p.BarCode).IsUnique();
            builder.Entity<MaterialRecord>().HasIndex(p => p.BarCode).IsUnique();

            //配置MaterialItem与Storage的一对多关系。 指定MaterialItem的StorageId是外键，切不能为空。
            //注意：Material的1对多；意思是Material关联了一个其余对象，而其余对象可以对应多个Material。
            //builder.Entity<MaterialItem>().HasOne<Storage>().WithMany().HasForeignKey(p => p.StorageId).IsRequired();  //无导航属性的配置方法：配置Material的1对多关系，其中Storageid是导航属性。
            //builder.Entity<MaterialItem>().HasIndex(p=>p.BarCode).IsUnique();
        }
    }
}




