namespace WarehouseManagementSystem.Service.TcpService
{
    public interface IClientManagerService
    {
        void AddClient(string clientEndpoint);
        void RemoveClient(string clientEndpoint);
        List<string> GetConnectedClients();
    }

    public class ClientManagerService : IClientManagerService
    {
        private readonly HashSet<string> _connectedClients = new();

        public void AddClient(string clientEndpoint)
        {
            _connectedClients.Add(clientEndpoint);
        }

        public void RemoveClient(string clientEndpoint)
        {
            _connectedClients.Remove(clientEndpoint);
        }

        public List<string> GetConnectedClients()
        {
            return _connectedClients.ToList();
        }
    }

}
