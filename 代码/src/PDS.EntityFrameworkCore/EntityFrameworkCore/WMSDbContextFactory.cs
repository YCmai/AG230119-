using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PDS.EntityFrameworkCore
{
    /* This class is needed for EF Core console commands
     * (like Add-Migration and Update-Database commands) */
    public class WMSDbContextFactory : IDesignTimeDbContextFactory<WMSDbContext>
    {
        public WMSDbContext CreateDbContext(string[] args)
        {

            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<WMSDbContext>()
                // .UseSqlServer(configuration.GetConnectionString("Default"),options=>options.EnableRetryOnFailure());
                .UseSqlServer(configuration.GetConnectionString("Default"));
            builder.ConfigureWarnings(w => w.Throw());

            return new WMSDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PDS.HttpApi.Host/"))   //这个类是用于迁移的，迁移时，把appsetting的目录改为了
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
