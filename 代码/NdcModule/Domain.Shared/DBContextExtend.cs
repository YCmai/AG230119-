using AciModule.Domain.Entitys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciModule.Domain.Shared
{
    public static class DBContextExtend
    {
        public static void ConfigureNdcTaskBaseModule(this ModelBuilder builder)
        {
            builder.Entity<NdcTask_Moves>(b =>
            {
                b.ToTable("NdcTask_Moves");
            });
        }
    }
}
