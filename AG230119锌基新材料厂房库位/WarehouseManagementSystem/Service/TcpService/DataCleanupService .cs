using WarehouseManagementSystem.Models.TcpService;

namespace WarehouseManagementSystem.Service.TcpService
{
    // Services/DataCleanupService.cs
    public class DataCleanupService : IHostedService, IDisposable
    {
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly ILogger<DataCleanupService> _logger;
        private readonly IConfiguration _configuration;
        private Timer? _timer;

        public class CleanupConfig
        {
            public int RetentionDays { get; set; } = 30;
            public int CleanupIntervalHours { get; set; } = 24;
        }

        public DataCleanupService(
            IMessageHistoryService messageHistoryService,
            ILogger<DataCleanupService> logger,
            IConfiguration configuration)
        {
            _messageHistoryService = messageHistoryService;
            _logger = logger;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 检查TCP服务是否启用
            var tcpServerEnabled = _configuration.GetValue<bool>("TcpServer:Enabled", true);
            if (!tcpServerEnabled)
            {
                _logger.LogInformation("TCP服务未启用，数据清理服务将不会启动。");
                return Task.CompletedTask;
            }

            _logger.LogInformation("数据清理服务正在启动。当前用户：{User}", "YCmai");

            var config = _configuration.GetSection("DataCleanup").Get<CleanupConfig>() ?? new CleanupConfig();

            _timer = new Timer(DoCleanup, null,
                TimeSpan.FromMinutes(1), // 延迟1分钟后开始
                TimeSpan.FromHours(config.CleanupIntervalHours));

            return Task.CompletedTask;
        }

        private async void DoCleanup(object? state)
        {
            // 每次清理前都检查服务状态
            var tcpServerEnabled = _configuration.GetValue<bool>("TcpServer:Enabled", true);
            if (!tcpServerEnabled)
            {
                _logger.LogInformation("TCP服务未启用，跳过数据清理操作。");
                return;
            }

            try
            {
                var config = _configuration.GetSection("DataCleanup").Get<CleanupConfig>() ?? new CleanupConfig();
                var cutoffDate = DateTime.UtcNow.AddDays(-config.RetentionDays);

                _logger.LogInformation(
                    "开始数据清理，当前时间：{CurrentTime}。清理时间早于 {CutoffDate} 的消息。",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    cutoffDate.ToString("yyyy-MM-dd HH:mm:ss"));

                if (_messageHistoryService is IMessageCleanupService cleanupService)
                {
                    // 检查是否有需要清理的数据
                    var messageCount = await cleanupService.GetMessageCountBeforeDateAsync(cutoffDate);
                    if (messageCount == 0)
                    {
                        _logger.LogInformation("未找到需要清理的消息。");
                        return;
                    }

                    await cleanupService.CleanupMessagesBeforeDateAsync(cutoffDate);
                    _logger.LogInformation("成功清理了 {Count} 条消息。", messageCount);
                }
                else
                {
                    _logger.LogWarning("消息服务未实现清理接口。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据清理时发生错误。");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("数据清理服务正在停止。");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
