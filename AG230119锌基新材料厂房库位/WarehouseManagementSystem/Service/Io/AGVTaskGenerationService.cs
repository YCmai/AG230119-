using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Service.Io
{
    public class AGVTaskGenerationService : BackgroundService
    {
        private readonly ILogger<AGVTaskGenerationService> _logger;
        private readonly StartupService _startupService;

        public AGVTaskGenerationService(
            ILogger<AGVTaskGenerationService> logger,
            StartupService startupService)
        {
            _logger = logger;
            _startupService = startupService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AGV任务生成服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _startupService.ProcessTasks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "生成AGV任务时发生错误");
                }

                await Task.Delay(2000, stoppingToken); // 每2秒执行一次
            }
        }
    }
} 