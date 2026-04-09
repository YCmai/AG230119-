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
using WMS.MaterialModule.Domain.Shared;
using TaskBaseModule.Domain.Shared;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using WMS.StorageModule.Domain.Shared;
using AciModule;
using AciModule.Domain.Shared;

namespace PDS.EntityFrameworkCore
{
    [ReplaceDbContext(typeof(IIdentityDbContext))]
    //[ReplaceDbContext(typeof(IStorageDbContext))]
    [ReplaceDbContext(typeof(ITenantManagementDbContext))]
    [ConnectionStringName("Default")]
    public class WMSDbContext : 
        AbpDbContext<WMSDbContext>,
        IIdentityDbContext,
        ITenantManagementDbContext
        //IStorageDbContext
    {
        /* Add DbSet properties for your Aggregate Roots / Entities here. */
        
        #region Entities from the modules
        
        /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
         * and replaced them for this DbContext. This allows you to perform JOIN
         * queries for the entities of these modules over the repositories easily. You
         * typically don't need that for other modules. But, if you need, you can
         * implement the DbContext interface of the needed module and use ReplaceDbContext
         * attribute just like IIdentityDbContext and ITenantManagementDbContext.
         *
         * More info: Replacing a DbContext of a module ensures that the related module
         * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
         */
        
        //Identity
        public DbSet<IdentityUser> Users { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<IdentityClaimType> ClaimTypes { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
        public DbSet<IdentityLinkUser> LinkUsers { get; set; }
        
        // Tenant Management
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }


       


        //WMS.StorageModule
        public DbSet<WMS.StorageModule.Domain.Storage> Storages { get; set; }
        public DbSet<WMS.StorageModule.Domain.WareHouse> WareHouses { get; set; }

        //WMS.BarCodeModule
        public DbSet<WMS.BarCodeModule.Domain.BarCodeRecord> BarCodeRecords { get; set; }

        //WMS.MaterialModule
        public DbSet<WMS.MaterialModule.Domain.MaterialStatistics> MaterialStatistics { get; set; }
        public DbSet<WMS.MaterialModule.Domain.MaterialItem> MaterialItems { get; set; }
        public DbSet<WMS.MaterialModule.Domain.MaterialInfo> MaterialInfos { get; set; }
        public DbSet<WMS.MaterialModule.Domain.MaterialRecord> MaterialRecords { get; set; }
        public DbSet<WMS.MaterialModule.Domain.MaterialModifyRecord> MaterialModifyRecords { get; set; }

        //WMS.LineCall
        public DbSet<WMS.LineCallInputModule.Domain.LineCallInputOrder> LineCallInputOrders { get; set; }
        public DbSet<WMS.LineCallInputModule.Domain.LineCallOutputOrder> LineCallOutputOrders { get; set; }

        //TaskBaseModule
        public DbSet<TaskBaseModule.Domain.TaskTrackInfo> TaskTracks { get; set; }
        public DbSet<TaskBaseModule.Domain.UserTask> UserTasks { get; set; }

        //HBTaskModule
        public DbSet<HBTaskModule.Domain.HBTask_Allocation> HBTask_Allocations { get; set; }
        public DbSet<HBTaskModule.Domain.HBTask_Load> HBTask_Loads { get; set; }
        public DbSet<HBTaskModule.Domain.HBTask_Move> HBTask_Moves { get; set; }
        public DbSet<HBTaskModule.Domain.HBTask_UnLoad> HBTask_UnLoads { get; set; }
        public DbSet<HBTaskModule.Domain.HBTask_Release> HBTask_Releases { get; set; }

        //WMS.ProcessControlModule
        public DbSet<WMS.LineCallProcessTaskModule.Domain.LineCallInputTask> LineCallInputTasks { get; set; }
        public DbSet<WMS.LineCallProcessTaskModule.Domain.LineCallOutputTask> LineCallOutputTasks { get; set; }

        //NDCTaskModule
        public DbSet<AciModule.Domain.Entitys.NdcTask_Moves> NdcTask_Moves { get; set; }



        //新引入
        public DbSet<AciModule.Domain.Entitys.RCS_WmsTask> RCS_WmsTask { get; set; }

        public DbSet<AciModule.Domain.Entitys.RCS_UserTasks> RCS_UserTasks { get; set; }

        public DbSet<AciModule.Domain.Entitys.RCS_IOAGV_Tasks> RCS_IOAGV_Tasks { get; set; }
        public DbSet<AciModule.Domain.Entitys.RCS_IODevices> RCS_IODevices { get; set; }
        public DbSet<AciModule.Domain.Entitys.RCS_IOSignals> RCS_IOSignals { get; set; }

        public DbSet<AciModule.Domain.Entitys.RCS_Locations> RCS_Locations { get; set; }




        public DbSet<AciModule.Domain.Entitys.RCS_ApiTask> RCS_ApiTask { get; set; }

        public DbSet<AciModule.Domain.Entitys.RCS_TaskErrRecord> RCS_TaskErrRecord { get; set; }

       
        #endregion

        public WMSDbContext(DbContextOptions<WMSDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);

            /* Include modules to your migration db context */

            builder.ConfigurePermissionManagement();
            builder.ConfigureSettingManagement();
            builder.ConfigureBackgroundJobs();
            builder.ConfigureAuditLogging();
            builder.ConfigureIdentity();
            //builder.ConfigureIdentityServer();
            builder.ConfigureFeatureManagement();
            builder.ConfigureTenantManagement();

            /* Configure your own tables/entities inside here */

            //builder.Entity<YourEntity>(b =>
            //{
            //    b.ToTable(PDSConsts.DbTablePrefix + "YourEntities", PDSConsts.DbSchema);
            //    b.ConfigureByConvention(); //auto configure for the base class props
            //    //...
            //});



            ////WMS.MaterialModule
            builder.ConfigureMaterialModule();

            //TaskBaseModule
            builder.ConfigureTaskBaseModule();

            //storage
            builder.ConfigureStorageModule();

            //NdcTask
            builder.ConfigureNdcTaskBaseModule();

        }


        //public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] {
        //    new DebugLoggerProvider()
        //});

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseLoggerFactory(MyLoggerFactory);
        //}
    }



}
