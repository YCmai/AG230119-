using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Service.Io
{
    public class IOMonitorService : BackgroundService
    {
        private readonly ILogger<IOMonitorService> _logger;
        private readonly StartupService _startupService;

        public IOMonitorService(
            ILogger<IOMonitorService> logger,
            StartupService startupService)
        {
            _logger = logger;
            _startupService = startupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IO监控服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _startupService.UpdateIOSignals();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新IO信号时发生错误");
                }

                await Task.Delay(500, stoppingToken); // 每500ms更新一次
            }
        }
    }
} 