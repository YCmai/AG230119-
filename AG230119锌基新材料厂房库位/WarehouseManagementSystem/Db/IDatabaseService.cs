using Microsoft.Data.SqlClient;
using System.Data;

namespace WarehouseManagementSystem.Db
{
    // Services/DatabaseService.cs
    public interface IDatabaseService
    {
        IDbConnection CreateConnection();
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
