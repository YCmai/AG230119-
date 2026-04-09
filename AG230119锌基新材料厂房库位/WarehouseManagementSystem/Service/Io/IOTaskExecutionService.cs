using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Service.Io
{
    public class IOTaskExecutionService : BackgroundService
    {
        private readonly ILogger<IOTaskExecutionService> _logger;
        private readonly StartupService _startupService;

        public IOTaskExecutionService(
            ILogger<IOTaskExecutionService> logger,
            StartupService startupService)
        {
            _logger = logger;
            _startupService = startupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IO任务执行服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _startupService.ExecuteIoTask();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行IO任务时发生错误");
                }

                await Task.Delay(1000, stoppingToken); // 每1秒执行一次
            }
        }
    }
} 