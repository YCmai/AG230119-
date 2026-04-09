using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models.TcpClient;

namespace WarehouseManagementSystem.Controllers
{
    public class TcpClientController : Controller
    {
        private readonly IConfiguration _configuration;

        public TcpClientController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new TcpClientModel
            {
                ServerIp = _configuration.GetValue<string>("TcpServer:Host", "127.0.0.1"),
                ServerPort = _configuration.GetValue<int>("TcpServer:Port", 9999),
                CurrentUser = User.Identity?.Name ?? "YCmai"
            };

            return View(model);
        }
    }
}
