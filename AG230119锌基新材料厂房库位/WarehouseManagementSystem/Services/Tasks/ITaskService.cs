using System.Net.Sockets;
using System.Net;
using NModbus;
using WarehouseManagementSystem.Hubs.TcpClient.Hubs;
using System.Data;
using WarehouseManagementSystem.Models.IO;
using WarehouseManagementSystem.Db;
using Dapper;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WarehouseManagementSystem.Hubs;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System;

public interface ITaskService
{
    Task<(List<RCS_UserTasks> Items, int TotalItems)> GetUserTasks(
        int page = 1, 
        int pageSize = 10, 
        DateTime? filterDate = null, 
        DateTime? endDate = null);
    Task<(bool success, string message)> CancelTask(int id);
}

public class TaskService : ITaskService
{
    private readonly IDatabaseService _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IDatabaseService db,
        IConfiguration configuration,
        ILogger<TaskService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(List<RCS_UserTasks> Items, int TotalItems)> GetUserTasks(
        int page = 1,
        int pageSize = 10,
        DateTime? filterDate = null,
        DateTime? endDate = null)
    {
        try
        {
            using var conn = _db.CreateConnection();

            var query = "SELECT * FROM RCS_UserTasks WHERE taskStatus < @CancelStatus";
            var countQuery = "SELECT COUNT(*) FROM RCS_UserTasks WHERE taskStatus < @CancelStatus";
            var parameters = new DynamicParameters();
            parameters.Add("@CancelStatus", (int)TaskStatuEnum.Canceled);

            if (filterDate.HasValue)
            {
                query += " AND endTime >= @FilterDate";
                countQuery += " AND endTime >= @FilterDate";
                parameters.Add("@FilterDate", filterDate.Value);
            }

            if (endDate.HasValue)
            {
                query += " AND endTime <= @EndDate";
                countQuery += " AND endTime <= @EndDate";
                parameters.Add("@EndDate", endDate.Value.AddDays(1));
            }

            query += " ORDER BY creatTime DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("@Offset", (page - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            var items = await conn.QueryAsync<RCS_UserTasks>(query, parameters);
            var totalItems = await conn.ExecuteScalarAsync<int>(countQuery, parameters);

            return (items.ToList(), totalItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取任务列表失败");
            return (new List<RCS_UserTasks>(), 0); // 返回空列表而不是抛出异常
        }
    }

    public async Task<(bool success, string message)> CancelTask(int id)
    {
        try
        {
            using var conn = _db.CreateConnection();
            
            // 检查任务是否存在且可以取消
            var task = await conn.QueryFirstOrDefaultAsync<RCS_UserTasks>(
                "SELECT * FROM RCS_UserTasks WHERE Id = @Id", 
                new { Id = id });

            if (task == null)
            {
                return (false, "任务不存在");
            }

            if (task.taskStatus >=  TaskStatuEnum.TaskFinish)
            {
                return (false, "已完成的任务不能取消");
            }

            if (task.IsCancelled)
            {
                return (false, "任务已经被取消");
            }

            // 更新任务状态为已取消
            var result = await conn.ExecuteAsync(@"
                UPDATE RCS_UserTasks 
                SET IsCancelled = 1
                WHERE Id = @Id",
                new { 
                    Id = id,
                });

            if (result > 0)
            {
                _logger.LogInformation($"任务 {id} 已成功取消");
                return (true, "任务已成功取消");
            }
            else
            {
                _logger.LogWarning($"任务 {id} 取消失败");
                return (false, "任务取消失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"取消任务 {id} 时发生错误");
            return (false, $"取消任务失败：{ex.Message}");
        }
    }

    private async Task RevertCancelStatus(int id)
    {
        try
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE RCS_UserTasks 
                SET IsCancelled = 0 
                WHERE Id = @Id",
                new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复任务取消状态失败");
        }
    }

    private (string baseUrl, string port, string http) GetConnectionParameters()
    {
        try
        {
            var baseUrl = _configuration["ConnectionStrings:IPAddress"];
            var port = _configuration["ConnectionStrings:Port"];
            var http = _configuration["ConnectionStrings:Http"];
            return (baseUrl, port, http);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接参数失败");
            return (string.Empty, string.Empty, string.Empty);
        }
    }
}
