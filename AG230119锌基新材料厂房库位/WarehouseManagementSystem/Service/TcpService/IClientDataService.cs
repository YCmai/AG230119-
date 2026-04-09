using WarehouseManagementSystem.Models.TcpService;

namespace WarehouseManagementSystem.Service.TcpService
{
    public interface IClientDataService
    {
        Task AddClientMessageAsync(string? clientEndpoint, string message);
        List<RCS_TcpClientMessages> GetClientMessages(int page, int pageSize);
        List<RCS_TcpClientMessages> GetClientMessagesByClient(string? clientEndpoint, int page, int pageSize);
        int GetTotalMessageCount();
    }

    public class ClientDataService : IClientDataService
    {
        private readonly List<RCS_TcpClientMessages> _messages = new();

        public Task AddClientMessageAsync(string? clientEndpoint, string message)
        {
            _messages.Add(new RCS_TcpClientMessages
            {
                ClientEndpoint = clientEndpoint,
                Message = message,
                Timestamp = DateTime.Now
            });
            return Task.CompletedTask;
        }

        public List<RCS_TcpClientMessages> GetClientMessages(int page, int pageSize)
        {
            return _messages
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public List<RCS_TcpClientMessages> GetClientMessagesByClient(string? clientEndpoint, int page, int pageSize)
        {
            return _messages
                .Where(m => m.ClientEndpoint == clientEndpoint)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalMessageCount()
        {
            return _messages.Count;
        }
    }


}
