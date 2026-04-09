using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace PDS.Data
{
    /* This is used if database provider does't define
     * IPDSDbSchemaMigrator implementation.
     */
    public class NullPDSDbSchemaMigrator : IPDSDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}