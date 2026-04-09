using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WarehouseManagementSystem.Controllers
{
    // 控制器
    [Route("api/[controller]")]
    [ApiController]
    public class TcpSetServerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TcpSetServerController> _logger;

        public TcpSetServerController(IConfiguration configuration, ILogger<TcpSetServerController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var enabled = _configuration.GetValue<bool>("TcpServer:Enabled", true);
            return Ok(new { enabled });
        }

        [HttpPost("toggle")]
        public IActionResult ToggleServer([FromBody] bool enabled)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var jsonString = System.IO.File.ReadAllText(filePath);
                var jsonObj = JObject.Parse(jsonString);

                // 更新 TcpServer:Enabled 值
                jsonObj["TcpServer"]["Enabled"] = enabled;

                // 写入文件
                System.IO.File.WriteAllText(filePath, jsonObj.ToString(Formatting.Indented));

                _logger.LogInformation($"TCP服务器状态已更新为: {enabled}");
                return Ok(new { success = true, message = $"TCP服务器已{(enabled ? "启用" : "禁用")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新TCP服务器状态时出错");
                return StatusCode(500, new { success = false, message = "更新配置失败" });
            }
        }


    }
}
