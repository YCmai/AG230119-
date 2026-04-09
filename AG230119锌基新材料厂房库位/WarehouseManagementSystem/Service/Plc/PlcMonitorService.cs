using HslCommunication.Profinet.Keyence;
using WarehouseManagementSystem.Models.PLC;

public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Dictionary<string, KeyenceMcNet> _plcConnections = new();
    private readonly object _lockObj = new();
    private const int RECONNECT_INTERVAL = 3000; // 重连间隔5秒
    private const int CONNECTION_TIMEOUT = 2000; // 连接超时2秒

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private KeyenceMcNet GetOrCreatePlcConnection(string ipAddress, int port)
    {
        var key = $"{ipAddress}:{port}";
        lock (_lockObj)
        {
            if (_plcConnections.TryGetValue(key, out var existingPlc))
            {
                return existingPlc;
            }

            try
            {
                var plc = new KeyenceMcNet(ipAddress, port);

               

                plc.ConnectTimeOut = CONNECTION_TIMEOUT;
                var connectResult = plc.ConnectServer();
                if (connectResult.IsSuccess)
                {
                    _plcConnections[key] = plc;
                    _logger.LogInformation($"成功连接PLC - IP: {ipAddress}:{port}");
                    return plc;
                }

                _logger.LogError($"连接PLC失败 - IP: {ipAddress}:{port}, 错误: {connectResult.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建PLC连接异常 - IP: {ipAddress}:{port}");
                return null;
            }
        }
    }

    private async Task<bool> ReconnectPlcAsync(string ipAddress, int port)
    {
        var key = $"{ipAddress}:{port}";

        try
        {
            // 先移除旧连接
            if (_plcConnections.TryGetValue(key, out var oldPlc))
            {
                await Task.Run(() => oldPlc.ConnectClose());
                _plcConnections.Remove(key);
            }

            // 等待重连间隔
            await Task.Delay(RECONNECT_INTERVAL);

            // 尝试重新连接
            var plc = new KeyenceMcNet(ipAddress, port);

            plc.ConnectTimeOut = CONNECTION_TIMEOUT;
            var connectResult = await Task.Run(() => plc.ConnectServer());

            if (connectResult.IsSuccess)
            {
                lock (_lockObj)
                {
                    _plcConnections[key] = plc;
                }
                _logger.LogInformation($"PLC重连成功 - IP: {ipAddress}:{port}");
                return true;
            }

            _logger.LogError($"PLC重连失败 - IP: {ipAddress}:{port}, 错误: {connectResult.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"PLC重连异常 - IP: {ipAddress}:{port}");
            return false;
        }
    }

    private async Task ProcessPlcGroupAsync(IGrouping<dynamic, PlcAddress> group, IPlcService plcService, CancellationToken stoppingToken)
    {
        try
        {
            var plc = GetOrCreatePlcConnection(group.Key.IpAddress, group.Key.Port);
            if (plc == null)
            {
                foreach (var address in group)
                {
                    await plcService.UpdatePlcAddressValueAsync(address.Id, "连接异常");
                    _logger.LogWarning(
                        $"PLC连接失败 - IP: {group.Key.IpAddress}, 地址: {address.Address}, " +
                        $"旧值: {address.CurrentValue}, 新值: 连接异常");
                }
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var address in group)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        try
                        {
                            string formattedAddress = FormatPlcAddress(address.Address);
                            var readResult = await Task.Run(() => plc.ReadString(formattedAddress, 1), stoppingToken);

                            if (address.Name.Contains("二维码") || address.Name.Contains("品番号"))
                            {
                                readResult = await Task.Run(() => plc.ReadString(formattedAddress, 20), stoppingToken);
                            }

                            if (readResult.IsSuccess)
                            {
                                string newValue = readResult.Content;

                                // 检查是否包含特定格式的数据（如：015B-141157-90050）
                                if (!string.IsNullOrEmpty(newValue) && 
                                    (newValue.Contains("-") || newValue.Length > 15)) // 根据实际情况调整长度判断
                                {
                                    // 清理乱码和空格
                                    newValue = new string(newValue.Where(c =>
                                        char.IsLetterOrDigit(c) ||
                                        c == '-' ||
                                        c == '_' ||
                                        c == '.').ToArray());
                                }

                                await plcService.UpdatePlcAddressValueAsync(address.Id, newValue);
                            }
                            else
                            {
                                await plcService.UpdatePlcAddressValueAsync(address.Id, "读取失败");
                                _logger.LogWarning(
                                    $"读取PLC值失败 - IP: {group.Key.IpAddress}, 地址: {address.Address}, " +
                                    $"错误: {readResult.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            await plcService.UpdatePlcAddressValueAsync(address.Id, "读取异常");
                            _logger.LogError(ex,
                                $"读取PLC地址异常 - IP: {group.Key.IpAddress}, 地址: {address.Address}");
                        }
                    }

                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    foreach (var address in group)
                    {
                        await plcService.UpdatePlcAddressValueAsync(address.Id, "连接异常");
                    }
                    _logger.LogError(ex, $"PLC通信异常 - IP: {group.Key.IpAddress}");

                    if (!await ReconnectPlcAsync(group.Key.IpAddress, group.Key.Port))
                    {
                        await Task.Delay(RECONNECT_INTERVAL, stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常的取消操作
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理PLC组异常 - IP: {group.Key.IpAddress}");
            foreach (var address in group)
            {
                await plcService.UpdatePlcAddressValueAsync(address.Id, "连接异常");
            }
        }
    }

    // 添加地址格式化方法
    private string FormatPlcAddress(string address)
    {
        // 移除所有空格
        address = address.Replace(" ", "").ToUpper();
        
        // 如果地址以字母开头（如DM），确保后面的数字部分正确
        if (address.StartsWith("DM"))
        {
            // 提取数字部分
            string numberPart = address.Substring(2);
            if (int.TryParse(numberPart, out int number))
            {
                return $"D{number}"; // Keyence格式：D后跟数字
            }
        }
        
        return address;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var plcService = scope.ServiceProvider.GetRequiredService<IPlcService>();
                var addresses = await plcService.GetMonitoringAddressesAsync();

                var plcGroups = addresses.GroupBy(a => new { a.IpAddress, a.Port });
                var tasks = plcGroups.Select(group => ProcessPlcGroupAsync(group, plcService, stoppingToken));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PLC监控服务执行失败");
            }

            await Task.Delay(1000, stoppingToken);
        }

        // 服务停止时关闭所有连接
        foreach (var plc in _plcConnections.Values)
        {
            try
            {
                await Task.Run(() => plc.ConnectClose());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭PLC连接异常");
            }
        }
        _plcConnections.Clear();
    }
}