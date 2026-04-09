using HslCommunication;
using HslCommunication.Profinet.Keyence;

using Microsoft.EntityFrameworkCore;

using WarehouseManagementSystem.Models.PLC;


/// <summary>
/// PLC任务监控
/// </summary>
public class PlcTaskExecuteService : BackgroundService
{
    private readonly ILogger<PlcTaskExecuteService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Dictionary<string, KeyenceMcNet> _plcConnections;

    public PlcTaskExecuteService(
        ILogger<PlcTaskExecuteService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _plcConnections = new Dictionary<string, KeyenceMcNet>();
    }

    private async Task<KeyenceMcNet> GetOrCreatePlcConnectionAsync(PlcAddress plcAddress)
    {
        var connectionKey = $"{plcAddress.IpAddress}:{plcAddress.Port}";

        if (_plcConnections.TryGetValue(connectionKey, out var existingPlc))
        {
            return existingPlc;
        }

        try
        {
            var plc = new KeyenceMcNet(plcAddress.IpAddress, plcAddress.Port);
            plc.ConnectTimeOut = 2000; // 设置连接超时时间

            // 使用同步方法进行连接
            var connect = plc.ConnectServer();
            if (!connect.IsSuccess)
            {
                _logger.LogError($"PLC连接失败 - IP: {plcAddress.IpAddress}, Port: {plcAddress.Port}, 错误: {connect.Message}");
                return null;
            }

            _logger.LogInformation($"PLC连接成功 - IP: {plcAddress.IpAddress}, Port: {plcAddress.Port}");
            _plcConnections[connectionKey] = plc;
            return plc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"初始化PLC连接时发生错误 - IP: {plcAddress.IpAddress}, Port: {plcAddress.Port}");
            return null;
        }
    }

    private async Task<(bool Success, string ErrorMessage)> VerifyWriteValueAsync(
        KeyenceMcNet plc, 
        string address, 
        string expectedValue)
    {
        try
        {
            // 使用 ReadString 读取当前值，与读取服务保持一致
            var readResult = await Task.Run(() => plc.ReadString(address, 1));
            if (!readResult.IsSuccess)
            {
                return (false, $"验证读取失败: {readResult.Message}");
            }

            var actualValue = readResult.Content;
            if (string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                return (true, null);
            }

            return (false, $"写入值验证失败: 期望值 {expectedValue}, 实际值 {actualValue}");
        }
        catch (Exception ex)
        {
            return (false, $"验证过程发生异常: {ex.Message}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var plcService = scope.ServiceProvider.GetRequiredService<IPlcService>();
                    var unexecutedTasks = await plcService.GetUnexecutedInteractionsAsync();

                    // 并行处理任务，每个任务独立执行
                    var tasks = unexecutedTasks.Select(async task =>
                    {
                        try
                        {
                            await ProcessPlcTaskAsync(task, plcService);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"处理PLC任务 {task.Id} 时发生错误");
                            await plcService.UpdateInteractionStatusAsync(task.Id, false, ex.Message);
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PLC交互服务执行失败");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessPlcTaskAsync(PlcInteraction task, IPlcService plcService)
    {
        if (task.PlcAddress == null)
        {
            _logger.LogWarning($"任务 {task.Id} 找不到对应的PLC地址信息");
            return;
        }

        // 获取或创建PLC连接
        var plc = await GetOrCreatePlcConnectionAsync(task.PlcAddress);
        if (plc == null)
        {
            await plcService.UpdateInteractionStatusAsync(
                task.Id, false, "PLC连接失败");
            return;
        }

        try
        {
            

            string valueToWrite = task.NewValue ?? "";

            // 格式化PLC地址
            string formattedAddress = FormatPlcAddress(task.PlcAddress.Address);
            
            // 使用 WriteString 方法写入字符串值
            var writeResult = await Task.Run(() => plc.Write(formattedAddress, valueToWrite));
            if (!writeResult.IsSuccess)
            {
                _logger.LogWarning(
                    $"PLC写入失败 - IP: {task.PlcAddress.IpAddress}, 地址: {task.PlcAddress.Address}, 错误: {writeResult.Message}");
                await plcService.UpdateInteractionStatusAsync(
                    task.Id, false, writeResult.Message);
                return;
            }

            // 验证写入值
            var verifyResult = await VerifyWriteValueAsync(plc, formattedAddress, valueToWrite);
            if (!verifyResult.Success)
            {
                _logger.LogWarning(
                    $"PLC写入验证失败 - IP: {task.PlcAddress.IpAddress}, 地址: {task.PlcAddress.Address}, {verifyResult.ErrorMessage}");
                await plcService.UpdateInteractionStatusAsync(
                    task.Id, false, verifyResult.ErrorMessage);
                return;
            }

            _logger.LogInformation(
                $"PLC写入成功并验证通过 - IP: {task.PlcAddress.IpAddress}, 地址: {task.PlcAddress.Address}, 值: {(string.IsNullOrEmpty(valueToWrite) ? "空值" : valueToWrite)}");

            await plcService.UpdateInteractionStatusAsync(task.Id, true, null);
            //await plcService.UpdatePlcAddressValueAsync(task.PlcAddressId, valueToWrite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"PLC交互任务执行异常 - IP: {task.PlcAddress.IpAddress}, 地址: {task.PlcAddress.Address}");
            await plcService.UpdateInteractionStatusAsync(
                task.Id, false, ex.Message);
        }
    }



    // 添加相同的地址格式化方法
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
}


