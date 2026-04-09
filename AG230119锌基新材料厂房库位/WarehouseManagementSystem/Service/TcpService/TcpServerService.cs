namespace WarehouseManagementSystem.Service.TcpService
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    using Microsoft.AspNetCore.SignalR;
    using WarehouseManagementSystem.Hubs;
    using WarehouseManagementSystem.Models.TcpService;

    public class TcpServerService : BackgroundService
    {
        private readonly ILogger<TcpServerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly IConfiguration _configuration;

        public TcpServerService(ILogger<TcpServerService> logger, IServiceProvider serviceProvider, IConfiguration configuration, IMessageHistoryService messageHistoryService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _messageHistoryService = messageHistoryService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TcpListener? server = null;
            
            try 
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var enabled = _configuration.GetValue<bool>("TcpServer:Enabled", true);
                    
                    if (!enabled)
                    {
                        // 如果服务被禁用，关闭现有的服务器
                        if (server != null)
                        {
                            server.Stop();
                            server = null;
                            _logger.LogInformation("TCP 服务器已停止.");
                        }
                        
                        // 等待一段时间后再次检查配置
                        await Task.Delay(3000, stoppingToken);
                        continue;
                    }

                    // 如果服务器未启动且enabled为true，则启动服务器
                    if (server == null)
                    {
                        var port = _configuration.GetValue("TcpServer:Port", 5000);
                        var host = _configuration.GetValue("TcpServer:Host", "0.0.0.0");
                        
                        server = new TcpListener(IPAddress.Parse(host), port);
                        server.Start();
                        _logger.LogInformation($"TCP 服务器开始监听 {host}:{port}");
                    }

                    try
                    {
                        var client = await server.AcceptTcpClientAsync(stoppingToken);
                        var clientEndpoint = client.Client.RemoteEndPoint?.ToString();
                        _logger.LogInformation($"客户端已连接: {clientEndpoint}");

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await HandleClientAsync(client, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"处理客户端时出错 {clientEndpoint}");
                            }
                            finally
                            {
                                client.Close();
                                _logger.LogInformation($"客户端已断开连接: {clientEndpoint}");

                                using var scope = _serviceProvider.CreateScope();
                                var clientManager = scope.ServiceProvider.GetRequiredService<IClientManagerService>();
                                clientManager.RemoveClient(clientEndpoint ?? "");
                                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TcpHub>>();
                                await hubContext.Clients.All.SendAsync("UpdateClientList", clientManager.GetConnectedClients());
                            }
                        }, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "接受客户端连接时出错.");
                    }
                }
            }
            finally
            {
                // 确保服务停止时关闭服务器
                if (server != null)
                {
                    server.Stop();
                    _logger.LogInformation("TCP 服务器已停止.");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var messageHistoryService = scope.ServiceProvider.GetRequiredService<IMessageHistoryService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TcpHub>>();

            using var stream = client.GetStream();
            var buffer = new byte[1024];
            var clientEndpoint = client.Client.RemoteEndPoint?.ToString();

            // 添加客户端连接记录
            if (!string.IsNullOrEmpty(clientEndpoint))
            {
               await messageHistoryService.UpdateClientStatusAsync(clientEndpoint, true);
            }

            try
            {
                while (!stoppingToken.IsCancellationRequested && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, stoppingToken);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        _logger.LogInformation($"客户端数据 {clientEndpoint}: {message}");

                        if (!string.IsNullOrEmpty(clientEndpoint))
                        {
                            // 添加消息到历史记录
                           await messageHistoryService.AddMessageAsync(clientEndpoint, message);
                        }

                        // 回复客户端
                        string response = $"服务器: {message}";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseBytes, stoppingToken);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                // 更新客户端断开状态
                if (!string.IsNullOrEmpty(clientEndpoint))
                {
                    await messageHistoryService.UpdateClientStatusAsync(clientEndpoint, false);
                }
                client.Close();
            }
        }
    }
}
