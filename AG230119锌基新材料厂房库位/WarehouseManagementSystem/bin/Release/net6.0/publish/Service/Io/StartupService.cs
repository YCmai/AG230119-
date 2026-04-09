namespace WarehouseManagementSystem.Service.Io
{
    // StartupService.cs
    public class StartupService : IHostedService
    {
        private readonly IIOService _ioService;
        private readonly ILogger<StartupService> _logger;

        public StartupService(IIOService ioService, ILogger<StartupService> logger)
        {
            _ioService = ioService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IO监控服务启动中");

            // 启动设备监控
            await _ioService.StartDeviceMonitoring();

            _logger.LogInformation("IO监控服务启动完成");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IO监控服务停止");
            return Task.CompletedTask;
        }
    }
}
