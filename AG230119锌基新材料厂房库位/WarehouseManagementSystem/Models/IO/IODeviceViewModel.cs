using System.Net;

using Microsoft.EntityFrameworkCore;

namespace WarehouseManagementSystem.Models.IO
{
    // Models/IODevice.cs
    public class RCS_IODevices
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public virtual ICollection<RCS_IOSignals> Signals { get; set; }
    }

    // Models/IOSignal.cs
    public class RCS_IOSignals
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        /// <summary>
        /// 0重置，1闭合，-1异常
        /// </summary>
        public int Value {  get; set; }
        public virtual RCS_IODevices Device { get; set; }

    }

    // Models/AppDbContext.cs
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RCS_IODevices> IODevices { get; set; }
        public DbSet<RCS_IOSignals> IOSignals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RCS_IODevices>()
                .HasMany(d => d.Signals)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId);
        }
    }
}
