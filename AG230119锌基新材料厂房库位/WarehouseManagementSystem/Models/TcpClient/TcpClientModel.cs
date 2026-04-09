namespace WarehouseManagementSystem.Models.TcpClient
{
    public class TcpClientModel
    {
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 5000;
        public bool IsConnected { get; set; }
        public List<MessageLog> MessageHistory { get; set; } = new();
        public int TotalMessagesSent { get; set; }
        public int TotalMessagesReceived { get; set; }
        public DateTime LastConnectedTime { get; set; }
        public string CurrentUser { get; set; } = "YCmai"; // 默认用户
    }

    public class MessageLog
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageDirection Direction { get; set; }
        public string Sender { get; set; }
    }

    public enum MessageDirection
    {
        Sent,
        Received
    }
}
