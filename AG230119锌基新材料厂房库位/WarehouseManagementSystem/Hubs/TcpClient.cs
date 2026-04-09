namespace WarehouseManagementSystem.Hubs
{
    // Hubs/TcpClientHub.cs
    using System;
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    using WarehouseManagementSystem.Models.TcpClient;

    namespace TcpClient.Hubs
    {

        public class TcpClientHub : Hub
        {
            private readonly ILogger<TcpClientHub> _logger;
            private static readonly ConcurrentDictionary<string, System.Net.Sockets.TcpClient> _clients = new();
            private static readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();

            public TcpClientHub(ILogger<TcpClientHub> logger)
            {
                _logger = logger;
            }

            public async Task Connect(string serverIp, int serverPort)
            {
                try
                {
                    _logger.LogInformation("正在尝试连接到 {ServerIp}:{ServerPort}", serverIp, serverPort);

                    // 如果已经存在连接，先断开
                    if (_clients.TryGetValue(Context.ConnectionId, out var existingClient))
                    {
                        _logger.LogInformation("断开现有连接");
                        existingClient.Close();
                        _clients.TryRemove(Context.ConnectionId, out _);
                    }

                    // 创建新的TCP客户端
                    var client = new System.Net.Sockets.TcpClient();
                    await client.ConnectAsync(serverIp, serverPort);

                    _logger.LogInformation("TCP连接成功");

                    // 存储客户端连接
                    _clients[Context.ConnectionId] = client;

                    // 创建新的取消令牌源
                    var cts = new CancellationTokenSource();
                    _cancellationTokens.TryAdd(Context.ConnectionId, cts);

                    // 通知客户端连接成功
                    _logger.LogInformation("发送连接成功状态到客户端");
                    await Clients.Caller.SendAsync("updateClientConnectionStatus", true);

                    // 开始接收消息
                    _ = HandleClientCommunicationAsync(client, Context.ConnectionId, cts.Token);

                    _logger.LogInformation("连接处理完成");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "连接失败");
                    await Clients.Caller.SendAsync("updateClientConnectionStatus", false);
                    throw;
                }
            }

            private async Task HandleClientCommunicationAsync(System.Net.Sockets.TcpClient client, string connectionId, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("开始处理客户端通信");
                    using var stream = client.GetStream();
                    var buffer = new byte[4096];

                    while (!cancellationToken.IsCancellationRequested && client.Connected)
                    {
                        try
                        {
                            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            if (bytesRead == 0)
                            {
                                _logger.LogInformation("TCP连接已关闭");
                                break;
                            }

                            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            _logger.LogInformation("收到消息: {Message}", message);

                            await Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                            {
                                Message = message,
                                Timestamp = DateTime.UtcNow,
                                Direction = 1 // Received
                            }, cancellationToken);
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError(ex, "读取数据时发生IO错误");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "客户端通信处理异常");
                }
                finally
                {
                    _logger.LogInformation("清理连接资源");
                    if (_clients.TryRemove(connectionId, out _))
                    {
                        await Clients.Client(connectionId).SendAsync("updateClientConnectionStatus", false);
                    }
                    client.Close();
                }
            }


            // 添加发送消息的方法
            public async Task SendMessage(string message)
            {
                try
                {
                    _logger.LogInformation("正在发送消息 - ConnectionId: {ConnectionId}, Message: {Message}",
                        Context.ConnectionId, message);

                    if (!_clients.TryGetValue(Context.ConnectionId, out var client))
                    {
                        throw new Exception("TCP客户端未连接");
                    }

                    if (!client.Connected)
                    {
                        throw new Exception("TCP客户端已断开连接");
                    }

                    // 将消息转换为字节数组
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    // 发送消息
                    await client.GetStream().WriteAsync(messageBytes);

                    // 通知前端消息已发送
                    await Clients.Caller.SendAsync("ReceiveMessage", new
                    {
                        Message = message,
                        Timestamp = DateTime.UtcNow,
                        Direction = 0 // 0表示发送
                    });

                    _logger.LogInformation("消息发送成功 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送消息失败 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw;
                }
            }


            // 添加断开连接方法
            public async Task Disconnect()
            {
                try
                {
                    _logger.LogInformation("正在断开连接 - ConnectionId: {ConnectionId}", Context.ConnectionId);

                    if (_clients.TryRemove(Context.ConnectionId, out var client))
                    {
                        // 关闭TCP客户端连接
                        client.Close();
                        client.Dispose();

                        // 通知前端断开成功
                        await Clients.Caller.SendAsync("updateClientConnectionStatus", false);
                        _logger.LogInformation("TCP客户端已断开连接 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                    }
                    else
                    {
                        _logger.LogWarning("未找到要断开的TCP客户端 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "断开连接失败 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw;
                }
            }

            // 当SignalR连接断开时也清理TCP客户端
            public override async Task OnDisconnectedAsync(Exception exception)
            {
                if (_clients.TryRemove(Context.ConnectionId, out var client))
                {
                    client.Close();
                    client.Dispose();
                    _logger.LogInformation("SignalR断开连接时清理TCP客户端 - ConnectionId: {ConnectionId}", Context.ConnectionId);
                }

                await base.OnDisconnectedAsync(exception);
            }
        }

    }
}
