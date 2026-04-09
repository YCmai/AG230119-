using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Domain;

namespace TaskBaseModule.Domain.Shared
{
    public static class DBContextExtend
    {
        public static void ConfigureTaskBaseModule(this ModelBuilder builder)
        {
            //  builder.Entity<TaskTrackInfo>(b =>
            //{
            //    b.HasKey(t => new { t.TaskId, t.ParentTaskId });  //设置复合key
            //});

            builder.Entity<TaskTrackInfo>().HasKey(t => new { t.TaskId, t.ParentTaskId });
            builder.Entity<UserTask>(b =>
            {
                b.HasMany<Option>(p=>p.Options).WithOne().HasForeignKey(p=>p.UserTaskId);
            });
        }
    }

 
}




