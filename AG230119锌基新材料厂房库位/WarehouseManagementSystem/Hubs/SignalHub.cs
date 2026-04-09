using Microsoft.AspNetCore.SignalR;

using WarehouseManagementSystem.Service.Io;

namespace WarehouseManagementSystem.Hubs
{
    // SignalHub.cs
    public class SignalHub : Hub
    {
        private readonly IIOService _ioService;

        public SignalHub(IIOService ioService)
        {
            _ioService = ioService;
        }

        public async Task UpdateDeviceStatus(int deviceId, bool isEnabled)
        {
            await _ioService.UpdateDeviceMonitoring(deviceId, isEnabled);
        }

        // SignalHub.cs
        public async Task TaskStatusUpdated(RCS_IOAGV_Tasks task)
        {
            await Clients.All.SendAsync("TaskStatusUpdated", task);
        }
    }
}
