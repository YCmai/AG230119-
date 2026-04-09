using Dapper;

using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Models.WebApi;

namespace WarehouseManagementSystem.Service.WepApi
{
    public class WebApiTaskService : IWebApiTaskService
    {
        private readonly IDatabaseService _db;
        private readonly ILogger<WebApiTaskService> _logger;

        public WebApiTaskService(IDatabaseService db, ILogger<WebApiTaskService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PagedResult<RcsEcsTask>> GetPagedTasksAsync(int pageNumber, int pageSize)
        {
            try
            {
               
                using (var connection = _db.CreateConnection())
                {
                    var sql = @"
                SELECT COUNT(*) FROM RCS_EcsTasks;
                SELECT * FROM RCS_EcsTasks 
                ORDER BY CreateTime DESC 
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

                    using (var multi = await connection.QueryMultipleAsync(sql,
                        new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize }))
                    {
                        var totalItems = await multi.ReadFirstAsync<int>();
                        var items = (await multi.ReadAsync<RcsEcsTask>()).ToList();

                        var result = new PagedResult<RcsEcsTask>
                        {
                            Items = items,
                            TotalItems = totalItems,
                            PageSize = pageSize,
                            PageNumber = pageNumber,
                            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                        };

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged tasks");
                throw;
            }
        }

        public async Task<IEnumerable<RcsEcsTask>> GetUnexecutedTasksAsync()
        {
            try
            {
                using (var connection = _db.CreateConnection())
                {
                    var sql = "SELECT * FROM RCS_EcsTasks WHERE [Execute] = 0 AND RetryCount < 3";
                    return await connection.QueryAsync<RcsEcsTask>(sql);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询接口任务失败");
                throw;
            }
        }

        public async Task UpdateTaskAsync(RcsEcsTask task)
        {
            try
            {
                using (var connection = _db.CreateConnection())
                {
                    var sql = @"
                    UPDATE RCS_EcsTasks 
                    SET [Execute] = @Execute, 
                        ExecuteResult = @ExecuteResult, 
                        RetryCount = @RetryCount, 
                        UpdateTime = @UpdateTime 
                    WHERE Id = @Id";

                    await connection.ExecuteAsync(sql, task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                throw;
            }
        }
    }
}
