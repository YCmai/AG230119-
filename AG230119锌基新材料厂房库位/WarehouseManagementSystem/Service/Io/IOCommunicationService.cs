using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Service.Io
{
    public class IOCommunicationService : BackgroundService
    {
        private readonly ILogger<IOCommunicationService> _logger;
        private readonly StartupService _startupService;

        public IOCommunicationService(
            ILogger<IOCommunicationService> logger,
            StartupService startupService)
        {
            _logger = logger;
            _startupService = startupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IO通信服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 先更新IO状态
                    await _startupService.UpdateIOSignals();
                    
                    // 然后执行IO任务
                    await _startupService.ExecuteIoTask();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "IO通信服务执行出错");
                }

                await Task.Delay(500, stoppingToken); // 每500ms执行一次
            }
        }
    }
} 