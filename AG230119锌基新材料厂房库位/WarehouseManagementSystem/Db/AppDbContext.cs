namespace WarehouseManagementSystem.Db
{
    using global::WarehouseManagementSystem.Models.IO;
    // Models/IO/AppDbContext.cs
    using Microsoft.EntityFrameworkCore;

    namespace WarehouseManagementSystem.Models.IO
    {
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

                modelBuilder.Entity<RCS_IODevices>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.IP).IsRequired();
                    entity.Property(e => e.Name).HasMaxLength(100);
                    entity.HasMany(e => e.Signals)
                          .WithOne(e => e.Device)
                          .HasForeignKey(e => e.DeviceId)
                          .OnDelete(DeleteBehavior.Cascade);
                });

                modelBuilder.Entity<RCS_IOSignals>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.Description).HasMaxLength(200);
                });
            }
        }
    }
}
