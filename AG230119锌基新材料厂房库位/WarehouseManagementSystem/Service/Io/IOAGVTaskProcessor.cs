using System.Data;
using System.Net;

using Dapper;

using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Models.IO;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace WarehouseManagementSystem.Service.Io
{
    public class IOAGVTaskProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseService _db;
        private readonly ILogger<IOAGVTaskProcessor> _logger;
        private readonly IIOService _ioService;


        public IOAGVTaskProcessor(IServiceProvider serviceProvider, IDatabaseService db, IIOService ioService, ILogger<IOAGVTaskProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _db = db;
            _ioService = ioService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("开始IO交互信号");
            int consecutiveErrorCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        using var conn = _db.CreateConnection();

                        // 添加命令超时
                        var command = new CommandDefinition(
                            @"SELECT * FROM RCS_IOAGV_Tasks 
                            WHERE Status = 'Pending' 
                            AND CreatedTime > DATEADD(HOUR, -6, GETUTCDATE())
                            ORDER BY CreatedTime ASC",
                            commandTimeout: 5); // 5秒超时

                        var tasks = await conn.QueryAsync<RCS_IOAGV_Tasks>(command);

                        foreach (var task in tasks)
                        {
                            try
                            {
                                if (!Enum.TryParse<EIOAddress>(task.SignalAddress, out EIOAddress addressEnum))
                                {
                                    _logger.LogWarning($"无效的信号地址: IP={task.DeviceIP}, Address={task.SignalAddress}");
                                    continue;
                                }

                                bool success = false;
                                try
                                {
                                    switch (task.TaskType)
                                    {
                                        case "ArrivalNotify":
                                        case "PassComplete":
                                            // 首先读取当前值
                                            var currentValue = await _ioService.ReadSignal(task.DeviceIP, addressEnum);
                                            if (currentValue == task.Value)
                                            {
                                                // 如果当前值已经是目标值，直接标记为成功
                                                success = true;
                                                _logger.LogInformation($"信号已经是目标值 - TaskId: {task.Id}, Device: {task.DeviceIP}, Address: {task.SignalAddress}, Value: {task.Value}");
                                            }
                                            else
                                            {
                                                // 值不同时才写入
                                                await _ioService.WriteSignal(task.DeviceIP, addressEnum, task.Value);
                                                success = await VerifyIOSignal(task.DeviceIP, addressEnum, task.Value);
                                            }
                                            break;

                                        case "PassCheck":
                                            // 读取通行信号
                                            success = await _ioService.ReadSignal(task.DeviceIP, addressEnum);
                                            break;

                                        default:
                                            _logger.LogWarning($"未知的任务类型: {task.TaskType}, TaskId: {task.Id}");
                                            break;
                                    }
                                }
                                catch (Exception ioEx)
                                {
                                    _logger.LogError(ioEx, 
                                        "执行IO操作失败 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}",
                                        task.Id, task.TaskType, task.DeviceIP, task.SignalAddress);
                                }

                                try
                                {
                                    if (success)
                                    {
                                        await UpdateTaskStatus(conn, task.Id, true);
                                        _logger.LogInformation(
                                            "任务处理成功 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}, 耗时: {Duration}ms",
                                            task.Id, task.TaskType, task.DeviceIP, task.SignalAddress,
                                            (DateTime.Now - task.CreatedTime).TotalMilliseconds);
                                    }
                                    else
                                    {
                                        await UpdateTaskStatus(conn, task.Id, false);
                                        _logger.LogWarning(
                                            "任务处理未完成 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}",
                                            task.Id, task.TaskType, task.DeviceIP, task.SignalAddress);
                                    }
                                }
                                catch (Exception updateEx)
                                {
                                    _logger.LogError(updateEx, 
                                        "更新任务状态失败 - TaskId: {TaskId}, Success: {Success}",
                                        task.Id, success);
                                }
                            }
                            catch (Exception taskEx)
                            {
                                _logger.LogError(taskEx,
                                    "处理任务失败 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}",
                                    task.Id, task.TaskType, task.DeviceIP, task.SignalAddress);
                            }
                        }
                    }

                    // 成功执行后重置错误计数
                    consecutiveErrorCount = 0;
                }
                catch (TaskCanceledException)
                {
                    // 应用正在关闭，忽略此异常
                    _logger.LogInformation("IO任务处理服务正在关闭");
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrorCount++;
                    _logger.LogError(ex, $"处理AGV任务时发生错误 (第{consecutiveErrorCount}次连续错误)");
                    
                    // 如果连续错误次数过多，增加等待时间
                    if (consecutiveErrorCount > 3)
                    {
                        int delayMs = Math.Min(consecutiveErrorCount * 500, 5000); // 最多等待5秒
                        _logger.LogWarning($"连续错误次数过多，等待{delayMs/1000.0:F1}秒后重试");
                        await Task.Delay(delayMs, stoppingToken);
                    }
                }

                try
                {
                    await Task.Delay(500, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // 应用正在关闭，忽略此异常
                    _logger.LogInformation("IO任务处理服务正在关闭");
                    break;
                }
                catch (Exception delayEx)
                {
                    // 延迟出错不应该导致服务停止
                    _logger.LogError(delayEx, "延迟等待时发生错误");
                }
            }

            _logger.LogInformation("IO任务处理服务已停止");
        }

        private async Task<bool> VerifyIOSignal(string deviceIP, EIOAddress address, bool expectedValue)
        {
            try
            {
                // 尝试最多3次读取验证
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var actualValue = await _ioService.ReadSignal(deviceIP, address);
                        if (actualValue == expectedValue)
                        {
                            return true;
                        }
                    }
                    catch (Exception readEx)
                    {
                        _logger.LogError(readEx, $"验证IO信号读取失败 (第{i+1}次尝试) - Device: {deviceIP}, Address: {address}");
                    }
                    
                    await Task.Delay(100); // 短暂延迟后重试
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"验证IO信号失败 - Device: {deviceIP}, Address: {address}");
                return false;
            }
        }

        private async Task UpdateTaskStatus(IDbConnection conn, int taskId, bool isCompleted)
        {
            try
            {
                await conn.ExecuteAsync(
                    @"UPDATE RCS_IOAGV_Tasks 
                    SET Status = @Status, 
                        CompletedTime = @CompletedTime,
                        LastUpdatedTime = @LastUpdatedTime
                    WHERE Id = @Id",
                    new
                    {
                        Id = taskId,
                        Status = isCompleted ? "Completed" : "Pending",
                        CompletedTime = isCompleted ? DateTime.Now : (DateTime?)null,
                        LastUpdatedTime = DateTime.Now
                    },
                    commandTimeout: 5); // 添加5秒超时
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新任务状态失败 - TaskId: {taskId}, IsCompleted: {isCompleted}");
                throw; // 重新抛出异常，让上层处理
            }
        }
    }

    // AGV任务实体类
    public class RCS_IOAGV_Tasks
    {
        public int Id { get; set; }
        /// <summary>
        /// 任务类型：ArrivalNotify(到达通知), PassCheck(通行检查), PassComplete(通行完成)
        /// </summary>
        public string TaskType { get; set; }
        /// <summary>
        /// 任务状态：Pending(待处理), Completed(已完成)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// IO设备IP地址
        /// </summary>
        public string DeviceIP { get; set; }
        /// <summary>
        ///  IO信号地址
        /// </summary>
        public string SignalAddress { get; set; }

        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        ///  最后更新时间
        /// </summary>
        public DateTime? LastUpdatedTime { get; set; }


        public string TaskId { get; set; }


        public bool Value { get; set; }

    }

    public enum TaskType
    {
        ArrivalNotify,
        PassCheck,
        PassComplete
    }

    public enum TaskStatus
    {
        Pending,
        Completed,
        Failed
    }

}
