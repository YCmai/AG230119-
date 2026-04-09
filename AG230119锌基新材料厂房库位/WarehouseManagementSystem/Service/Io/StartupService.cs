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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Serilog.Core;
using static WarehouseManagementSystem.Service.Io.IOService;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
namespace WarehouseManagementSystem.Service.Io
{
    // StartupService.cs
    public class StartupService : BackgroundService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IDatabaseService _db;
        private readonly IHubContext<SignalHub> _hubContext;
        private readonly IIOService _ioService;
        private readonly ITaskGenerationService _taskGenerationService;
        private static readonly SemaphoreSlim _ioLock = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<string, DateTime> _di7SignalStartTimes = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentDictionary<string, DateTime> _downlineSignalStartTimes = new ConcurrentDictionary<string, DateTime>();

        public StartupService(
            ILogger<StartupService> logger,
            IDatabaseService db,
            IHubContext<SignalHub> hubContext,
            IIOService ioService,
            ITaskGenerationService taskGenerationService)
        {
            _logger = logger;
            _db = db;
            _hubContext = hubContext;
            _ioService = ioService;
            _taskGenerationService = taskGenerationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("开始监控IO设备");

            // 添加更新信号操作的同步锁
            SemaphoreSlim updateSignalsSemaphore = new SemaphoreSlim(1, 1);
            int consecutiveFailureCount = 0; // 添加连续失败次数计数器

            while (!stoppingToken.IsCancellationRequested)
            {
                bool hasError = false;
                bool lockAcquired = false;

                try
                {
                    // 1. 读取并更新所有IO信号
                    lockAcquired = await updateSignalsSemaphore.WaitAsync(0);
                    if (lockAcquired)
                    {
                        try
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);
                            
                            await UpdateIOSignals();
                           // _logger.LogInformation("完成执行UpdateIOSignals");
                            
                            // 2. 处理IO交互任务
                            await ExecuteIoTask();
                           // _logger.LogInformation("完成执行ExecuteIoTask");
                            
                            // 重置失败计数
                            consecutiveFailureCount = 0;
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogWarning("操作超时，已取消任务");
                            hasError = true;
                            consecutiveFailureCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "IO任务执行失败");
                            hasError = true;
                            consecutiveFailureCount++;
                        }
                        finally
                        {
                            // 确保在finally块中释放锁
                            updateSignalsSemaphore.Release();
                            lockAcquired = false;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("上一次操作尚未完成，跳过本次执行");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行IO监控任务失败");
                    hasError = true;
                    consecutiveFailureCount++;
                    
                    // 确保在外部catch块中释放锁（如果获取了但未释放）
                    if (lockAcquired)
                    {
                        try { updateSignalsSemaphore.Release(); } catch { }
                    }
                }

                // 连续失败次数过多时增加延迟
                if (consecutiveFailureCount > 5)
                {
                    _logger.LogWarning($"连续失败{consecutiveFailureCount}次，增加等待时间");
                    await Task.Delay(Math.Min(consecutiveFailureCount * 500, 10000), stoppingToken); // 最多等待10秒
                }
                else if (hasError)
                {
                    // 有错误时稍微延长等待时间
                    await Task.Delay(1000, stoppingToken);
                }
                else
                {
                    // 正常等待时间
                    await Task.Delay(500, stoppingToken);
                }
            }
        }


        public async Task UpdateIOSignals()
        {
            try
            {
                using var conn = _db.CreateConnection();

                // 先处理未启用的设备，将它们的信号全部设置为false
                var command = new CommandDefinition(
                    "SELECT * FROM RCS_IODevices WHERE IsEnabled = 0",
                    commandTimeout: 5); // 5秒超时
                var disabledDevices = await conn.QueryAsync<RCS_IODevices>(command);
                
                foreach (var device in disabledDevices)
                {
                    command = new CommandDefinition(
                        "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                        new { DeviceId = device.Id },
                        commandTimeout: 5);
                    var signals = await conn.QueryAsync<RCS_IOSignals>(command);

                    _logger.LogInformation($"设备{device.Name}({device.IP})未启用，将所有信号设置为false");
                    foreach (var signal in signals)
                    {
                        try
                        {
                            signal.UpdatedTime = DateTime.Now;
                            signal.Value = (int)IOSignalStatus.Error;
                            await conn.ExecuteAsync(
                                "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                new { signal.Value, signal.UpdatedTime, signal.Id });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"更新未启用设备{device.Name}({device.IP})信号{signal.Address}失败：{ex.Message}");
                        }
                    }
                }

                // 保持原有逻辑处理已启用的设备
                command = new CommandDefinition(
                    "SELECT * FROM RCS_IODevices WHERE IsEnabled = 1",
                    commandTimeout: 5);
                var devices = await conn.QueryAsync<RCS_IODevices>(command);

                foreach (var device in devices)
                {
                    try
                    {
                        // 添加防呆操作：先使用Ping检测设备IP是否可达
                        bool isReachable = false;
                        try
                        {
                            using Ping pinger = new Ping();
                            PingReply reply = await pinger.SendPingAsync(IPAddress.Parse(device.IP), 1000); // 1秒超时
                            isReachable = reply.Status == IPStatus.Success;
                            
                            if (!isReachable)
                            {
                                _logger.LogWarning($"设备{device.Name}({device.IP}) Ping失败，网络不可达，状态：{reply.Status}");
                                
                                // 如果Ping失败，将设备的所有信号设置为Error状态
                                command = new CommandDefinition(
                                    "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                                    new { DeviceId = device.Id },
                                    commandTimeout: 5);
                                var deviceSignals = await conn.QueryAsync<RCS_IOSignals>(command);
                                
                                foreach (var signal in deviceSignals)
                                {
                                    signal.UpdatedTime = DateTime.Now;
                                    signal.Value = (int)IOSignalStatus.Error;
                                    await conn.ExecuteAsync(
                                        "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                        new { signal.Value, signal.UpdatedTime, signal.Id });
                                }
                                
                                continue; // 跳过此设备，不尝试读取信号
                            }
                        }
                        catch (Exception pingEx)
                        {
                            _logger.LogError($"Ping设备{device.Name}({device.IP})失败：{pingEx.Message}");
                            // Ping失败也视为设备不可达，但继续执行，让下面的代码尝试连接
                        }

                        command = new CommandDefinition(
                            "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                            new { DeviceId = device.Id },
                            commandTimeout: 5);
                        var signals = await conn.QueryAsync<RCS_IOSignals>(command);

                        // 设备连接正常，逐个读取信号
                        foreach (var signal in signals)
                        {
                            try
                            {
                                if (!Enum.TryParse<EIOAddress>(signal.Address, out EIOAddress addressEnum))
                                {
                                    continue;
                                }

                                // 使用锁来读取信号，但添加超时
                                bool lockAcquired = false;
                                try
                                {
                                    // 设置5秒获取锁超时
                                    lockAcquired = await _ioLock.WaitAsync(5000);
                                    if (!lockAcquired)
                                    {
                                        _logger.LogWarning($"无法获取IO锁，跳过设备{device.Name}({device.IP})信号{signal.Address}读取");
                                        continue;
                                    }
                                    
                                    IOSignalStatus signalStatus;
                                    try
                                    {
                                        // 添加更长的读取超时
                                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                                        var readTask = _ioService.ReadSignal(device.IP, addressEnum);
                                        
                                        if (await Task.WhenAny(readTask, Task.Delay(3000, cts.Token)) == readTask)
                                        {
                                            var value = await readTask;
                                            signalStatus = value ? IOSignalStatus.On : IOSignalStatus.Off;
                                        }
                                        else
                                        {
                                            throw new TimeoutException($"读取设备{device.Name}({device.IP})信号{signal.Address}超时");
                                        }
                                        
                                        signal.UpdatedTime = DateTime.Now;
                                        signal.Value = (int)signalStatus;
                                        await conn.ExecuteAsync(
                                            "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                            new { signal.Value, signal.UpdatedTime, signal.Id },
                                            commandTimeout: 5);
                                    }
                                    catch (Exception signalEx) when (signalEx.Message.Contains("transaction ID"))
                                    {
                                        // 特殊处理事务ID错误
                                        _logger.LogWarning($"设备{device.Name}({device.IP})信号{signal.Address}发生事务ID错误：{signalEx.Message}");

                                        // 尝试重置连接后再次读取
                                        try
                                        {
                                            // 释放资源，等待一段时间让设备复位
                                            await Task.Delay(1000);

                                            // 再次尝试读取
                                            _logger.LogInformation($"正在尝试重新读取设备{device.Name}({device.IP})信号{signal.Address}");
                                            var retryValue = await _ioService.ReadSignal(device.IP, addressEnum);
                                            signalStatus = retryValue ? IOSignalStatus.On : IOSignalStatus.Off;
                                            signal.UpdatedTime = DateTime.Now;
                                            signal.Value = (int)signalStatus; ;
                                            await conn.ExecuteAsync(
                                                "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                                new { signal.Value, signal.UpdatedTime, signal.Id });

                                            _logger.LogInformation($"成功重新读取设备{device.Name}({device.IP})信号{signal.Address}");
                                        }
                                        catch (Exception retryEx)
                                        {
                                            _logger.LogError($"重试读取设备{device.Name}({device.IP})信号{signal.Address}失败：{retryEx.Message}");
                                            // 重试失败时，设置信号为false
                                            signal.UpdatedTime = DateTime.Now;
                                            signal.Value = (int)IOSignalStatus.Error;
                                            await conn.ExecuteAsync(
                                                "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                                new { signal.Value, signal.UpdatedTime, signal.Id });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"读取设备{device.Name}({device.IP})信号{signal.Address}失败：{ex.Message}");
                                        // 单个信号读取失败，设置为Error状态
                                        signal.UpdatedTime = DateTime.Now;
                                        signal.Value = (int)IOSignalStatus.Error;
                                        await conn.ExecuteAsync(
                                            "UPDATE RCS_IOSignals SET Value = @Value, UpdatedTime = @UpdatedTime WHERE Id = @Id",
                                            new { signal.Value, signal.UpdatedTime, signal.Id },
                                            commandTimeout: 5);
                                    }
                                }
                                finally
                                {
                                    // 确保在finally块中释放锁
                                    if (lockAcquired)
                                    {
                                        _ioLock.Release();
                                    }
                                }

                                // 每个信号读取后添加短暂延迟，避免发送过快造成设备不响应
                                await Task.Delay(100);
                            }
                            catch (Exception signalLoopEx)
                            {
                                _logger.LogError($"处理设备{device.Name}({device.IP})信号{signal.Address}时发生异常：{signalLoopEx.Message}");
                            }
                        }
                    }
                    catch (Exception deviceEx)
                    {
                        _logger.LogError($"处理设备{device.Name}({device.IP})时发生异常：{deviceEx.Message}");

                        // 设备整体异常处理，重置所有信号
                        try
                        {
                            await conn.ExecuteAsync(
                                @"UPDATE RCS_IOSignals 
                                SET Value = 0, UpdatedTime = @UpdatedTime 
                                WHERE DeviceId = @DeviceId",
                                new { DeviceId = device.Id, UpdatedTime = DateTime.Now },
                                commandTimeout: 5);

                            _logger.LogInformation($"已将设备{device.Name}({device.IP})的所有信号重置为false");
                        }
                        catch (Exception resetEx)
                        {
                            _logger.LogError($"重置设备{device.Name}({device.IP})信号失败：{resetEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"更新IO信号失败：{ex.Message}");
            }
        }


        public async Task ExecuteIoTask()
        {
            try
            {
                using var conn = _db.CreateConnection();
                
                // 添加命令超时参数
                var commandDefinition = new CommandDefinition(
                    @"SELECT IP FROM RCS_IODevices WHERE IsEnabled = 1",
                    commandTimeout: 5 // 5秒超时
                );

                // 先获取所有启用的设备IP列表
                var enabledDevices = await conn.QueryAsync<string>(commandDefinition);
                var enabledIPSet = new HashSet<string>(enabledDevices);

                // 设置查询Pending任务的超时
                commandDefinition = new CommandDefinition(
                    @"SELECT * 
                    FROM RCS_IOAGV_Tasks
                    WHERE Status = 'Pending'",
                    commandTimeout: 5 // 5秒超时
                );

                // 只查询Pending状态的任务
                var tasks = await conn.QueryAsync<RCS_IOAGV_Tasks>(commandDefinition);

                // 限制一次处理的任务数量
                var limitedTasks = tasks.Take(50).ToList();

                // 使用C#进行分组和排序
                var taskGroups = limitedTasks
                    .OrderBy(t => t.CreatedTime)
                    .GroupBy(t => t.DeviceIP);

                foreach (var group in taskGroups)
                {
                    var deviceIP = group.Key;

                    // 检查设备是否启用
                    if (!enabledIPSet.Contains(deviceIP))
                    {
                        _logger.LogWarning($"设备 {deviceIP} 未启用或不存在，跳过相关任务");

                        // 可选：将未启用设备的任务标记为失败
                        foreach (var task in group)
                        {
                            try
                            {
                                await conn.ExecuteAsync(
                                    @"UPDATE RCS_IOAGV_Tasks 
                                    SET Status = 'Failed', 
                                        LastUpdatedTime = @LastUpdatedTime,
                                        CompletedTime = @CompletedTime
                                    WHERE Id = @Id",
                                    new
                                    {
                                        Id = task.Id,
                                        LastUpdatedTime = DateTime.Now,
                                        CompletedTime = DateTime.Now
                                    },
                                    commandTimeout: 5 // 5秒超时
                                );
                                _logger.LogInformation($"已将设备 {deviceIP} 的任务 {task.Id} 标记为失败（设备未启用）");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"标记任务 {task.Id} 失败时出错");
                            }
                        }

                        continue; // 跳过此设备组的后续处理
                    }

                    _logger.LogInformation($"开始处理设备 {deviceIP} 的任务组");

                    foreach (var task in group)
                    {
                        try
                        {
                            if (!Enum.TryParse<EIOAddress>(task.SignalAddress, out EIOAddress addressEnum))
                            {
                                _logger.LogWarning($"无效的信号地址: IP={task.DeviceIP}, Address={task.SignalAddress}");
                                continue;
                            }

                            bool success = false;
                            bool lockAcquired = false;
                            
                            try
                            {
                                // 设置获取锁的超时（防止死锁）
                                lockAcquired = await _ioLock.WaitAsync(5000); // 5秒超时获取锁
                                
                                if (!lockAcquired)
                                {
                                    _logger.LogWarning($"无法获取IO锁，任务 {task.Id} 将稍后重试");
                                    continue;
                                }
                                
                                // 使用带超时的Task.Run包装IO操作
                                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5秒超时
                                
                                success = await ProcessIoTaskWithTimeout(task, addressEnum, cts.Token);
                            }
                            finally
                            {
                                // 确保在获取锁成功的情况下才释放
                                if (lockAcquired)
                                {
                                    _ioLock.Release();
                                }
                            }

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
                                // 如果任务失败，跳过该IP的后续任务
                                _logger.LogWarning(
                                    "任务处理失败，跳过该设备的后续任务 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}",
                                    task.Id, task.TaskType, task.DeviceIP, task.SignalAddress);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "处理任务失败，跳过该设备的后续任务 - TaskId: {TaskId}, Type: {TaskType}, Device: {DeviceIP}, Address: {Address}",
                                task.Id, task.TaskType, task.DeviceIP, task.SignalAddress);
                            break;
                        }
                    }
                }

                // 所有任务处理完成后，清理1天前的已完成任务
                try
                {
                    int deletedCount = await conn.ExecuteAsync(
                        @"DELETE FROM RCS_IOAGV_Tasks
                        WHERE Status = 'Completed'
                        AND CompletedTime < DATEADD(DAY, -1, GETUTCDATE())",
                        commandTimeout: 10 // 10秒超时
                    );

                    if (deletedCount > 0)
                    {
                        _logger.LogInformation($"已清理 {deletedCount} 条1天前的历史任务记录");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理历史任务记录时发生错误");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理IO任务失败");
            }
        }
        
        // 新增带超时的IO任务处理方法
        private async Task<bool> ProcessIoTaskWithTimeout(RCS_IOAGV_Tasks task, EIOAddress addressEnum, CancellationToken token)
        {
            try
            {
                switch (task.TaskType)
                {
                    case "ArrivalNotify":
                    case "PassComplete":
                    case "SetSignal":
                        // 设置读取操作的超时
                        var readTask = _ioService.ReadSignal(task.DeviceIP, addressEnum);
                        var plcCurrentValue = await WaitForTask(readTask, TimeSpan.FromSeconds(2), token);
                        
                        if (plcCurrentValue == task.Value)
                        {
                            // 如果当前值已经是目标值，添加短暂延迟后再次确认
                            await Task.Delay(300, token);
                            
                            // 再次读取进行确认
                            readTask = _ioService.ReadSignal(task.DeviceIP, addressEnum);
                            var secondReadValue = await WaitForTask(readTask, TimeSpan.FromSeconds(2), token);
                            
                            if (secondReadValue == task.Value)
                            {
                                return true;
                            }
                            else
                            {
                                _logger.LogWarning($"二次读取信号值不一致 - TaskId: {task.Id}, 第一次: {plcCurrentValue}, 第二次: {secondReadValue}, 目标值: {task.Value}");
                                
                                // 尝试写入
                                var writeTask = _ioService.WriteSignal(task.DeviceIP, addressEnum, task.Value);
                                await WaitForTask(writeTask, TimeSpan.FromSeconds(3), token);
                                
                                // 添加延迟后验证
                                await Task.Delay(300, token);
                                
                                readTask = _ioService.ReadSignal(task.DeviceIP, addressEnum);
                                var verifiedValue = await WaitForTask(readTask, TimeSpan.FromSeconds(2), token);
                                
                                return (verifiedValue == task.Value);
                            }
                        }
                        else
                        {
                            // 值不同时才写入
                            var writeTask = _ioService.WriteSignal(task.DeviceIP, addressEnum, task.Value);
                            await WaitForTask(writeTask, TimeSpan.FromSeconds(3), token);
                            
                            // 添加延迟后验证
                            await Task.Delay(300, token);
                            
                            readTask = _ioService.ReadSignal(task.DeviceIP, addressEnum);
                            var verifiedValue = await WaitForTask(readTask, TimeSpan.FromSeconds(2), token);
                            
                            if (verifiedValue == task.Value)
                            {
                                return true;
                            }
                            
                            // 如果第一次验证失败，再尝试一次
                            _logger.LogWarning($"第一次验证失败，再次尝试 - TaskId: {task.Id}, Device: {task.DeviceIP}, Address: {task.SignalAddress}");
                            await Task.Delay(300, token);
                            
                            readTask = _ioService.ReadSignal(task.DeviceIP, addressEnum);
                            verifiedValue = await WaitForTask(readTask, TimeSpan.FromSeconds(2), token);
                            
                            return (verifiedValue == task.Value);
                        }

                    case "PassCheck":
                        // 从数据库读取通行信号
                        using (var conn = _db.CreateConnection())
                        {
                            return await GetSignalValueFromDatabase(conn, task.DeviceIP, task.SignalAddress);
                        }

                    default:
                        _logger.LogWarning($"未知的任务类型: {task.TaskType}");
                        return false;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"处理任务 {task.Id} 超时取消");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理任务 {task.Id} 时发生错误");
                return false;
            }
        }
        
        // 辅助方法：带超时的Task等待
        private async Task<T> WaitForTask<T>(Task<T> task, TimeSpan timeout, CancellationToken token)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout, token)) == task)
            {
                return await task; // 任务在超时前完成
            }
            
            throw new TimeoutException($"操作超时（{timeout.TotalSeconds}秒）");
        }

        private async Task<bool> GetSignalValueFromDatabase(IDbConnection conn, string deviceIP, string signalAddress)
        {
            try
            {
                // 添加命令超时
                var commandDefinition = new CommandDefinition(
                    @"SELECT Id FROM RCS_IODevices 
                    WHERE IP = @DeviceIP AND IsEnabled = 1",
                    new { DeviceIP = deviceIP },
                    commandTimeout: 5  // 5秒超时
                );

                // 查询设备ID
                var deviceId = await conn.QueryFirstOrDefaultAsync<int>(commandDefinition);

                if (deviceId == 0)
                {
                    _logger.LogWarning($"无法找到设备: IP={deviceIP}");
                    return false;
                }

                // 查询信号值，同样添加超时
                commandDefinition = new CommandDefinition(
                    @"SELECT Value FROM RCS_IOSignals 
                    WHERE DeviceId = @DeviceId AND Address = @Address",
                    new { DeviceId = deviceId, Address = signalAddress },
                    commandTimeout: 5  // 5秒超时
                );

                var signalValue = await conn.QueryFirstOrDefaultAsync<bool?>(commandDefinition);

                if (signalValue == null)
                {
                    _logger.LogWarning($"无法找到信号: DeviceIP={deviceIP}, Address={signalAddress}");
                    return false;
                }

                return signalValue.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"从数据库获取信号值失败: DeviceIP={deviceIP}, Address={signalAddress}");
                return false;
            }
        }


        private async Task UpdateTaskStatus(IDbConnection conn, int taskId, bool isCompleted)
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
                });
        }


        private async Task<bool> VerifyIOSignal(string deviceIP, EIOAddress address, bool expectedValue)
        {
            try
            {
                // 尝试最多3次读取验证
                for (int i = 0; i < 3; i++)
                {
                    var actualValue = await _ioService.ReadSignal(deviceIP, address);
                    if (actualValue == expectedValue)
                    {
                        return true;
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




        public async Task ProcessTasks()
        {
            try
            {
                using var conn = _db.CreateConnection();
                var devices = await conn.QueryAsync<RCS_IODevices>(
                    "SELECT * FROM RCS_IODevices WHERE IsEnabled = 1");

                foreach (var device in devices)
                {
                    var signals = (await conn.QueryAsync<RCS_IOSignals>(
                        "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                        new { DeviceId = device.Id })).ToList();

                    if (device.Id <= 27) // 上料架任务
                    {
                        // 1. 首先检查 DI7 信号
                        var di7Signal = signals.FirstOrDefault(s => s.Address == "DI7");
                        if (di7Signal == null || di7Signal.Value != 1)
                        {
                            // DI7 信号不存在或未激活，清除该设备的计时
                            _di7SignalStartTimes.TryRemove(device.Id.ToString(), out _);
                            continue;
                        }

                        // 2. 检查 DI7 信号持续时间
                        var now = DateTime.Now;
                        var signalStartTime = _di7SignalStartTimes.GetOrAdd(device.Id.ToString(), now);
                        
                        if ((now - signalStartTime).TotalSeconds >= 1) // DI7 信号已持续 3 秒
                        {
                            // 3. 检查 DI1-DI6 的信号
                            var diSignals = signals.Where(s => 
                                s.Address.StartsWith("DI") && 
                                s.Address != "DI7" &&
                                int.TryParse(s.Address.Substring(2), out int num) && 
                                num <= 6 &&
                                s.Value == 0).ToList();

                            // 4. 为每个激活的信号生成任务
                            foreach (var signal in diSignals)
                            {
                                await _taskGenerationService.GenerateAGVTask(signal, device);
                            }
                        }
                    }
                    else if (device.Id >= 29) // 下料任务
                    {
                        foreach (var signal in signals)
                        {
                            if (signal.Value == 1 && signal.Address.StartsWith("DI"))
                            {
                                var signalKey = $"{device.Id}_{signal.Address}";
                                var now = DateTime.Now;

                                // 如果信号首次出现，记录开始时间
                                var signalStartTime = _downlineSignalStartTimes.GetOrAdd(signalKey, now);

                                // 检查信号是否持续了5秒
                                if ((now - signalStartTime).TotalSeconds >= 1)
                                {
                                    await _taskGenerationService.GenerateAGVTask(signal, device);
                                    // 任务生成后清除计时，避免重复生成
                                    _downlineSignalStartTimes.TryRemove(signalKey, out _);
                                }
                            }
                            else
                            {
                                // 如果信号变为OFF，清除对应的计时
                                _downlineSignalStartTimes.TryRemove($"{device.Id}_{signal.Address}", out _);
                            }
                        }
                    }
                }

                // 清理过期的信号记录（可选，防止字典无限增长）
                CleanupExpiredSignals();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务监控服务执行出错");
            }
        }

        private void CleanupExpiredSignals()
        {
            var now = DateTime.Now;
            var expiredTime = TimeSpan.FromMinutes(10); // 10分钟后清理

            // 清理上料架信号记录
            var expiredDi7Signals = _di7SignalStartTimes
                .Where(kvp => (now - kvp.Value) > expiredTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredDi7Signals)
            {
                _di7SignalStartTimes.TryRemove(key, out _);
            }

            // 清理下料信号记录
            var expiredDownlineSignals = _downlineSignalStartTimes
                .Where(kvp => (now - kvp.Value) > expiredTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredDownlineSignals)
            {
                _downlineSignalStartTimes.TryRemove(key, out _);
            }
        }
    }
}
