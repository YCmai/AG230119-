using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models.TcpService;
using WarehouseManagementSystem.Service.TcpService;

namespace WarehouseManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TcpMonitorController : ControllerBase
    {
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly IMessageCleanupService _cleanupService;
        public TcpMonitorController(IMessageHistoryService messageHistoryService, IMessageCleanupService cleanupService)
        {
            _messageHistoryService = messageHistoryService;
            _cleanupService = cleanupService;
        }

        [HttpGet("messages")]
        public IActionResult GetMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string client = null)
        {
            var messages = _messageHistoryService.GetMessages(page, pageSize, client);
            return Ok(messages);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var status = _messageHistoryService.GetClientStatusesAsync();
            return Ok(status);
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            start ??= DateTime.UtcNow.AddHours(-24);
            end ??= DateTime.UtcNow;
            var stats = _messageHistoryService.GetMessageStatistics(start.Value, end.Value);
            return Ok(stats);
        }


        [HttpPost("cleanup")]
        public async Task<IActionResult> TriggerCleanup([FromQuery] int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            await _cleanupService.CleanupMessagesBeforeDateAsync(cutoffDate);
            return Ok("Cleanup completed");
        }
    }
}
