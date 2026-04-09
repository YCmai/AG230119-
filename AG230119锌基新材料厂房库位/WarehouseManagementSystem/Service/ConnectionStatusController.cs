using Microsoft.AspNetCore.Mvc;

namespace WarehouseManagementSystem.Service
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConnectionStatusController : ControllerBase
    {
        private readonly ConnectionStatusService _connectionStatusService;

        public ConnectionStatusController(ConnectionStatusService connectionStatusService)
        {
            _connectionStatusService = connectionStatusService;
        }

        [HttpGet]
        public IActionResult GetConnectionStatus()
        {
            return Ok(new { isConnected = _connectionStatusService.IsConnected });
        }
    }



}
