using Microsoft.AspNetCore.SignalR;
namespace WarehouseManagementSystem.Hubs
{
    

    public class TcpHub : Hub
    {
        public async Task SendMessage(string clientEndpoint, string message)
        {
            // 广播消息给所有连接的用户
            await Clients.All.SendAsync("ReceiveMessage", clientEndpoint, message);
        }

        public async Task UpdateClients(List<string> connectedClients)
        {
            // 更新连接客户端列表
            await Clients.All.SendAsync("UpdateClientList", connectedClients);
        }
    }

}
