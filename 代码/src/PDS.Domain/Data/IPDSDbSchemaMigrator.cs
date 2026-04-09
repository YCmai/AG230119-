using System.Threading.Tasks;

namespace PDS.Data
{
    public interface IPDSDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
