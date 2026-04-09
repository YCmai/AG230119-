using System.Net.Sockets;
using System.Net;
using NModbus;
using WarehouseManagementSystem.Hubs.TcpClient.Hubs;
using System.Data;
using WarehouseManagementSystem.Models.IO;
using WarehouseManagementSystem.Db;
using Dapper;
using System.Collections.Concurrent;

namespace WarehouseManagementSystem.Service.Io
{
    // Services/IIOService.cs
    public interface IIOService
    {
        Task<bool> Conn(string ip);
        Task<bool> ReadSignal(string ip, EIOAddress address);
        Task<bool> WriteSignal(string ip, EIOAddress address, bool value);
        List<Remote_IO_Info> GetConnectedClients();

        Task StartDeviceMonitoring();
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
        private readonly ConcurrentDictionary<string, Task> _monitoringTasks;

        public IOService(
            ILogger<IOService> logger, IDatabaseService db, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _db = db;
            _serviceProvider = serviceProvider;
            _monitoringTasks = new ConcurrentDictionary<string, Task>();
        }

        public List<Remote_IO_Info> GetConnectedClients()
        {
            return io_List;
        }

        public async Task<bool> Conn(string ip)
        {
            try
            {
                _logger.LogInformation($"IO_【{ip}】尝试连接");
                var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

                if (remoteInfo == null)
                {
                    remoteInfo = new Remote_IO_Info { IP = ip };
                    io_List.Add(remoteInfo);
                }

                if (remoteInfo.Master_TcpClient != null && !remoteInfo.Master_TcpClient.Connected)
                {
                    remoteInfo.Master_TcpClient.Close();
                    remoteInfo.Master_TcpClient = null;
                }

                remoteInfo.NModbus = new ModbusFactory();
                remoteInfo.Master_TcpClient = new TcpClient();
                await remoteInfo.Master_TcpClient.ConnectAsync(ip, 502);

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
            }
            return false;
        }

        public async Task<bool> ReadSignal(string ip, EIOAddress address)
        {
            await _writeSemaphore.WaitAsync();
            try
            {
                return await ReadSignalImpl(ip, address);
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }

        private async Task<bool> ReadSignalImpl(string ip, EIOAddress address)
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
                        return false;
                    }
                    remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);
                }

                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                    {
                        var readTask = remoteInfo.Master.ReadCoilsAsync(1, (ushort)address, 1);
                        var result = await Task.WhenAny(readTask, Task.Delay(2000, cts.Token));

                        if (result == readTask)
                        {
                            return (await readTask)[0];
                        }
                        throw new TimeoutException("读取超时");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IO_【{ip}】读取失败：{ex.Message}");
                    await Conn(ip);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IO_【{ip}】_ReadSignal读取异常：{ex.Message}");
            }
            return false;
        }

        public async Task<bool> WriteSignal(string ip, EIOAddress address, bool value)
        {
            await _writeSemaphore.WaitAsync();
            try
            {
                return await WriteSignalImpl(ip, address, value);
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }

        private async Task<bool> WriteSignalImpl(string ip, EIOAddress address, bool value)
        {
            const int maxRetries = 2;
            int retryCount = 0;

            while (retryCount <= maxRetries)
            {
                try
                {
                    var remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);

                    if (remoteInfo?.Master_TcpClient?.Client?.Connected != true ||
                        !await CheckConnection(remoteInfo.Master_TcpClient))
                    {
                        _logger.LogInformation($"IO_【{ip}】准备重新连接 (尝试 {retryCount + 1}/{maxRetries + 1})");
                        bool isConnected = await Conn(ip);
                        if (!isConnected)
                        {
                            if (retryCount == maxRetries)
                            {
                                _logger.LogError($"IO_【{ip}】连接失败，无法写入信号");
                                return false;
                            }
                            retryCount++;
                            continue;
                        }
                        remoteInfo = io_List.FirstOrDefault(m => m.IP == ip);
                    }

                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                    {
                        try
                        {
                            var writeTask = remoteInfo.Master.WriteSingleCoilAsync(1, (ushort)address, value);
                            await Task.WhenAny(writeTask, Task.Delay(2000, cts.Token));

                            if (!writeTask.IsCompleted)
                            {
                                throw new TimeoutException("写入操作超时");
                            }

                            await writeTask;
                            _logger.LogInformation($"IO_【{ip}】地址{address}写入{value}成功");
                            return true;
                        }
                        catch (Exception ex) when (ex is IOException || ex is TimeoutException)
                        {
                            if (retryCount == maxRetries)
                            {
                                throw;
                            }

                            _logger.LogError($"IO_【{ip}】写入失败 (尝试 {retryCount + 1}/{maxRetries + 1})：{ex.Message}");

                            try
                            {
                                remoteInfo.Master_TcpClient?.Close();
                                remoteInfo.Master_TcpClient?.Dispose();
                            }
                            catch { }

                            retryCount++;
                            await Task.Delay(500 * (retryCount));
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IO_【{ip}】_WriteSignal写入异常：{ex.Message}");
                    if (retryCount == maxRetries)
                    {
                        return false;
                    }
                    retryCount++;
                    await Task.Delay(500 * (retryCount));
                }
            }

            return false;
        }

        private async Task<bool> CheckConnection(TcpClient client)
        {
            try
            {
                if (client?.Client == null) return false;
                if (!client.Connected) return false;

                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
                {
                    try
                    {
                        return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task StartDeviceMonitoring()
        {

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            // 获取所有启用的设备
            var enabledDevices = await db.QueryAsync<RCS_IODevices>(
                "SELECT * FROM RCS_IODevices WHERE IsEnabled = 1");

            foreach (var device in enabledDevices)
            {
                StartMonitoringDevice(device);
            }
        }

        private void StartMonitoringDevice(RCS_IODevices device)
        {
            var monitoringTask = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // 获取设备的所有信号
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                        var signals = await db.QueryAsync<RCS_IOSignals>(
                            "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                            new { DeviceId = device.Id });

                        foreach (var signal in signals)
                        {
                            try
                            {

                                if (!Enum.TryParse<EIOAddress>(signal.Address, out EIOAddress addressEnum))
                                {
                                    _logger.LogWarning($"无效的信号地址:{signal.Address}");
                                }

                                // 读取信号状态
                                var value = await ReadSignal(device.IP, addressEnum);

                                // 发送SignalR通知前端
                                await NotifySignalStatusChanged(device.IP, signal.Address, value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "读取信号失败: IP={IP}, Address={Address}",
                                    device.IP, signal.Address);
                            }
                        }

                        await Task.Delay(1000); // 每秒刷新一次
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "监控设备出错: {Device}", device.IP);
                        await Task.Delay(5000); // 出错后等待5秒再重试
                    }
                }
            });

            _monitoringTasks.TryAdd(device.IP, monitoringTask);
        }
    }
}
