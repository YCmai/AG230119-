namespace WarehouseManagementSystem.Service.TcpService
{
    // Services/IMessageCleanupService.cs
    public interface IMessageCleanupService
    {
        Task<int> GetMessageCountBeforeDateAsync(DateTime cutoffDate);
        Task CleanupMessagesBeforeDateAsync(DateTime cutoffDate);
    }
}
