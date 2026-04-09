namespace WarehouseManagementSystem.Models.TcpService
{
    // ViewModels/MessageViewModel.cs
    public class MessageViewModel
    {
        public List<RCS_TcpClientMessages> Messages { get; set; }
        public List<RCS_TcpClientStatuses> ClientStatuses { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalMessages { get; set; }
        public string SelectedClient { get; set; }
        public Dictionary<DateTime, int> Statistics { get; set; }

        public ProcessStatusStatistics ProcessStatistics { get; set; }
    }

    public class ProcessStatusStatistics
    {
        public int TotalMessages { get; set; }
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public List<RCS_TcpClientMessages> RecentFailedMessages { get; set; }
    }
}
