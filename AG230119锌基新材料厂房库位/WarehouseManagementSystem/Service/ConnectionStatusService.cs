public class ConnectionStatusService
{
    private readonly IConfiguration _configuration;
    private bool _isConnected;
    private readonly Timer _timer;

    public ConnectionStatusService(IConfiguration configuration)
    {
        _configuration = configuration;
        _isConnected = false;
        // 每隔 5 秒检查一次连接状态
        _timer = new Timer(CheckConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    public bool IsConnected => _isConnected;

    private async void CheckConnection(object state)
    {
        var ip = _configuration["ConnectionStrings:IPAddress"];
        var port = int.Parse(_configuration["ConnectionStrings:Port"]);

        _isConnected = await CheckConnection(ip, port);
    }

    public async Task<bool> CheckConnection(string ip, int port)
    {
        using var client = new System.Net.Sockets.TcpClient();
        try
        {
            await client.ConnectAsync(ip, port);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ForceCheckConnection()
    {
        var ip = _configuration["ConnectionStrings:IPAddress"];
        var port = int.Parse(_configuration["ConnectionStrings:Port"]);

        _isConnected = await CheckConnection(ip, port);
    }
}
