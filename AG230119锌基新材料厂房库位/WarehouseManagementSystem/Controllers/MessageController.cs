using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models.TcpService;
using WarehouseManagementSystem.Service.TcpService;

namespace WarehouseManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageHistoryService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageHistoryService messageService, ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetMessageStatus([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var statistics = await _messageService.GetMessageStatisticsAsync();
                var messages = await _messageService.GetPagedMessagesAsync(page, pageSize);
                var totalCount = await _messageService.GetTotalMessageCountAsync();

                return Ok(new
                {
                    statistics = new
                    {
                        pendingCount = statistics.PendingCount,
                        processingCount = statistics.ProcessingCount,
                        completedCount = statistics.CompletedCount,
                        failedCount = statistics.FailedCount
                    },
                    messages = messages,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalRecords = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取消息状态时发生错误");
                return StatusCode(500, new { error = "获取消息状态失败" });
            }
        }

        // 添加API端点用于获取分页数据
        [HttpGet]
        public IActionResult GetMessages(int page = 1, int pageSize = 10, string clientEndpoint = null)
        {
            var messages = _messageService.GetMessages(page, pageSize, clientEndpoint);
            var total = _messageService.GetTotalMessageCount(clientEndpoint);

            return Ok(new
            {
                data = messages,
                total = total,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }
    }
}
