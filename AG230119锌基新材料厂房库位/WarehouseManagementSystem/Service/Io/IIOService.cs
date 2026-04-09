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


namespace WarehouseManagementSystem.Service.Io
{
    // Services/IIOService.cs
    public interface IIOService
    {
        Task<bool> Conn(string ip);
        Task<bool> ReadSignal(string ip, EIOAddress address);
        Task<bool> WriteSignal(string ip, EIOAddress address, bool value);
        List<Remote_IO_Info> GetConnectedClients();

        Task UpdateDeviceMonitoring(int deviceId, bool isEnabled);

        // 新增添加IO任务的接口
        Task<int> AddIOTask(string taskType, string deviceIP, string signalAddress, bool value, string taskId);

    }

    // Models/Remote_IO_Info.cs
    public class Remote_IO_Info
    {
        public string IP { get; set; }
        public ModbusFactory NModbus { get; set; }
        public TcpClient Master_TcpClient { get; set; }
        public IModbusMaster Master { get; set; }
    }

    // Services/IOService.cs
    public class IOService : IIOService
    {
        private readonly ILogger<IOService> _logger;
        private readonly List<Remote_IO_Info> io_List = new();
        private readonly SemaphoreSlim _writeSemaphore = new(1, 1);
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _serviceProvider;
        private readonly IIODeviceService _ioDeviceService;
        private readonly IHubContext<SignalHub> _hubContext;
        // 定义 _monitoringTasks
        private readonly ConcurrentDictionary<string, (Task, CancellationTokenSource)> _monitoringTasks = new();
        private readonly ITaskGenerationService _taskGenerationService;

        public IOService(
            ILogger<IOService> logger, IDatabaseService db, IIODeviceService ioDeviceService, IServiceProvider serviceProvider, IHubContext<SignalHub> hubContext, ITaskGenerationService taskGenerationService)
        {
            _logger = logger;
            _db = db;
            _serviceProvider = serviceProvider;
            _monitoringTasks = new ConcurrentDictionary<string, (Task, CancellationTokenSource)>();
            _hubContext = hubContext;
            _ioDeviceService = ioDeviceService;
            _taskGenerationService = taskGenerationService;
        }

        public List<Remote_IO_Info> GetConnectedClients()
        {
            return io_List;
        }

        public async Task<int> AddIOTask(string taskType, string deviceIP, string signalAddress, bool value, string taskId)
        {
            try
            {
                using var conn = _db.CreateConnection();
                var task = new RCS_IOAGV_Tasks
                {
                    TaskType = taskType,
                    Status = "Pending",
                    DeviceIP = deviceIP,
                    SignalAddress = signalAddress,
                    Value = value,
                    TaskId = taskId,
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now
                };

                var sql = @"INSERT INTO RCS_IOAGV_Tasks 
                    (TaskType, Status, DeviceIP, SignalAddress, Value, TaskId, CreatedTime, LastUpdatedTime) 
                    VALUES 
                    (@TaskType, @Status, @DeviceIP, @SignalAddress, @Value, @TaskId, @CreatedTime, @LastUpdatedTime);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                var newTaskId = await conn.ExecuteScalarAsync<int>(sql, task);
                _logger.LogInformation($"创建IO任务成功: ID={newTaskId}, Type={taskType}, Device={deviceIP}");
                return newTaskId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建IO任务失败");
                throw;
            }
        }



        // 更新设备状态时重启监控
        public async Task UpdateDeviceMonitoring(int deviceId, bool isEnabled)
        {
            using var conn = _db.CreateConnection();

            var device = await conn.QueryFirstOrDefaultAsync<RCS_IODevices>(
                "SELECT * FROM RCS_IODevices WHERE Id = @Id",
                new { Id = deviceId });

            if (device == null) return;

            if (isEnabled)
            {
                  
            }
            else
            {
                // 停止设备监控
                if (_monitoringTasks.TryRemove(device.IP, out var taskInfo))
                {
                    var (task, cts) = taskInfo;
                    cts.Cancel(); // 取消任务
                    try
                    {
                        await task; // 等待任务完成
                        cts.Dispose(); // 释放资源
                    }
                    catch (OperationCanceledException)
                    {
                        // 任务被取消
                    }

                    _logger.LogInformation(
                        "设备监控已停止: ID={ID}, IP={IP}, Time={Time}, User={User}",
                        device.Id, device.IP, DateTime.UtcNow, "YCmai");
                }
            }
        }

        public async Task<bool> Conn(string ip)
        {
            try
            {
                _logger.LogInformation($"IO_【{ip}】尝试连接");
                var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

                // 先清理旧的连接资源
                if (remoteInfo != null)
                {
                    try
                    {
                        if (remoteInfo.Master_TcpClient != null)
                        {
                            remoteInfo.Master_TcpClient.Close();
                            remoteInfo.Master_TcpClient.Dispose();
                        }
                        if (remoteInfo.Master != null)
                        {
                            remoteInfo.Master.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"IO_【{ip}】清理旧连接资源时发生错误：{ex.Message}");
                    }
                    io_List.Remove(remoteInfo);
                }

                // 创建新的连接
                remoteInfo = new Remote_IO_Info { IP = ip };
                io_List.Add(remoteInfo);

                try
                {
                    remoteInfo.NModbus = new ModbusFactory();
                    remoteInfo.Master_TcpClient = new TcpClient();

                    // 设置连接超时
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    await remoteInfo.Master_TcpClient.ConnectAsync(ip, 502).WaitAsync(cts.Token);

                    if (remoteInfo.Master_TcpClient.Connected)
                    {
                        remoteInfo.Master = remoteInfo.NModbus.CreateMaster(remoteInfo.Master_TcpClient);
                        remoteInfo.Master.Transport.ReadTimeout = 2000;
                        remoteInfo.Master.Transport.Retries = 2000;
                        _logger.LogInformation($"IO_【{ip}】_连接成功");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IO_【{ip}】连接尝试失败，错误：{ex.Message}");

                    // 清理失败连接的资源
                    try
                    {
                        if (remoteInfo.Master_TcpClient != null)
                        {
                            remoteInfo.Master_TcpClient.Close();
                            remoteInfo.Master_TcpClient.Dispose();
                        }
                        if (remoteInfo.Master != null)
                        {
                            remoteInfo.Master.Dispose();
                        }
                        io_List.Remove(remoteInfo);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning($"IO_【{ip}】清理失败连接资源时发生错误：{cleanupEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IO_【{ip}】连接过程发生异常：{ex.Message}");
            }
            return false;
        }


        public async Task<bool> ReadSignal(string ip, EIOAddress address)
        {
            //await _writeSemaphore.WaitAsync();
            //try
            //{
                return await ReadSignalImpl(ip, address);
            //}
            //finally
            //{
            //    _writeSemaphore.Release();
            //}
        }

        private async Task<bool> ReadSignalImpl(string ip, EIOAddress address)
        {
            try
            {
                var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

                // 只有在以下情况才需要重新连接：
                // 1. 没有连接信息
                // 2. 连接已断开
                // 3. 连接检查失败
                if (remoteInfo?.Master_TcpClient?.Client?.Connected != true ||
                    !await CheckConnection(remoteInfo.Master_TcpClient))
                {
                    _logger.LogInformation($"IO_【{ip}】连接已断开或无效，尝试重新连接");
                    bool isConnected = await Conn(ip);
                    if (!isConnected)
                    {
                        _logger.LogWarning($"IO_【{ip}】重新连接失败");
                        return false;
                    }
                    remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);
                }

                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // 1秒读取超时
                    var readTask = remoteInfo.Master.ReadCoilsAsync(1, (ushort)address, 1);
                    var completedTask = await Task.WhenAny(readTask, Task.Delay(1000, cts.Token));

                    if (completedTask != readTask)
                    {
                        _logger.LogWarning($"IO_【{ip}】读取超时");
                        // 读取超时，但不立即断开连接
                        return false;
                    }

                    var result = (await readTask)[0];
                   // _logger.LogDebug($"IO_【{ip}】读取成功：{address} = {result}"); // 使用Debug级别避免日志过多
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IO_【{ip}】读取失败：{ex.Message}");
                    // 读取失败时，标记连接需要检查，但不立即断开
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IO_【{ip}】_ReadSignal读取异常：{ex.Message}");
            }
            return false;
        }

        private async Task<bool> CheckConnection(TcpClient client)
        {
            try
            {
                if (client?.Client == null) return false;
                if (!client.Connected) return false;

                // 快速检查连接状态，设置较短的超时时间
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                try
                {
                    // 使用 Poll 方法检查连接状态
                    if (client.Client.Poll(1, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        // 如果可读但长度为0，说明连接已断开
                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> WriteSignal(string ip, EIOAddress address, bool value)
        {
           // await _writeSemaphore.WaitAsync();
            //try
            //{
                return await WriteSignalImpl(ip, address, value);
            //}
            //finally
            //{
            //    _writeSemaphore.Release();
            //}
        }

        private async Task<bool> WriteSignalImpl(string ip, EIOAddress address, bool value)
        {
            try
            {
                var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

                if (remoteInfo?.Master_TcpClient?.Client?.Connected != true ||
                    !await CheckConnection(remoteInfo.Master_TcpClient))
                {
                    bool isConnected = await Conn(ip);
                    if (!isConnected)
                    {
                        _logger.LogWarning($"IO_【{ip}】连接失败");
                        return false;
                    }
                    remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);
                }

                // 增加重试机制
                for (int retry = 0; retry < 3; retry++)
                {
                    try
                    {
                        await remoteInfo.Master.WriteSingleCoilAsync(1, (ushort)address, value);
                        _logger.LogInformation($"IO_【{ip}】地址{address}写入{value}成功");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (retry == 2)
                        {
                            _logger.LogError($"IO_【{ip}】写入失败（最后一次尝试）：{ex.Message}");
                            // 最后一次尝试失败时重新连接
                            await Conn(ip);
                            return false;
                        }

                        _logger.LogWarning($"IO_【{ip}】写入失败（第{retry + 1}次尝试）：{ex.Message}");
                        await Task.Delay(200 * (retry + 1)); // 递增延迟

                        try
                        {
                            remoteInfo.Master_TcpClient?.Close();
                            remoteInfo.Master_TcpClient?.Dispose();
                        }
                        catch { }

                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IO_【{ip}】_WriteSignal写入异常：{ex.Message}");
            }
            return false;
        }



        //private async Task<bool> WriteSignalImpl(string ip, EIOAddress address, bool value)
        //{
        //    const int maxRetries = 2;
        //    int retryCount = 0;



        //    while (retryCount <= maxRetries)
        //    {
        //        try
        //        {
        //            var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

        //            // 只在必要时重新连接
        //            if (remoteInfo?.Master_TcpClient?.Client?.Connected != true ||
        //                !await CheckConnection(remoteInfo.Master_TcpClient))
        //            {
        //                _logger.LogInformation($"IO_【{ip}】准备重新连接 (尝试 {retryCount + 1}/{maxRetries + 1})");
        //                bool isConnected = await Conn(ip);
        //                if (!isConnected)
        //                {
        //                    if (retryCount == maxRetries)
        //                    {
        //                        _logger.LogError($"IO_【{ip}】连接失败，无法写入信号");
        //                        return false;
        //                    }
        //                    retryCount++;
        //                    await Task.Delay(500 * (retryCount));
        //                    continue;
        //                }
        //                remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);
        //            }

        //            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
        //            {
        //                try
        //                {
        //                    var writeTask = remoteInfo.Master.WriteSingleCoilAsync(1, (ushort)address, value);
        //                    var completedTask = await Task.WhenAny(writeTask, Task.Delay(2000, cts.Token));

        //                    if (completedTask != writeTask)
        //                    {
        //                        throw new TimeoutException("写入操作超时");
        //                    }

        //                    await writeTask;
        //                    _logger.LogInformation($"IO_【{ip}】地址{address}写入{value}成功");
        //                    return true;
        //                }
        //                catch (Exception ex) when (ex is IOException || ex is TimeoutException)
        //                {
        //                    if (retryCount == maxRetries)
        //                    {
        //                        throw;
        //                    }

        //                    _logger.LogError($"IO_【{ip}】写入失败 (尝试 {retryCount + 1}/{maxRetries + 1})：{ex.Message}");
        //                    retryCount++;
        //                    await Task.Delay(500 * (retryCount));
        //                    continue;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"IO_【{ip}】_WriteSignal写入异常：{ex.Message}");
        //            if (retryCount == maxRetries)
        //            {
        //                return false;
        //            }
        //            retryCount++;
        //            await Task.Delay(500 * (retryCount));
        //        }
        //    }

        //    return false;
        //}

        // 修改枚举值
        public enum IOSignalStatus
        {
            Error = -1,     // 错误状态（如连接失败、读取失败等）
            Off = 0,        // 关闭状态 (false)
            On = 1          // 开启状态 (true)
        }

        // 修改 StartDeviceMonitoring 方法中的相关代码

    }
}
