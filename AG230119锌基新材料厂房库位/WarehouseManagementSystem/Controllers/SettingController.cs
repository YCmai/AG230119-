using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Controllers
{
    public class SettingController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionStatusService _connectionStatusService;
        public SettingController(IConfiguration configuration, ConnectionStatusService connectionStatusService, ISystemExpirationService expirationService)
            : base(expirationService)
        {
            _configuration = configuration;
            _connectionStatusService = connectionStatusService;
        }


        public IActionResult Index()
        {
            var settings = _configuration.GetSection("ConnectionStrings").Get<ConnectionSettings>();
            return View(settings);
        }

        public IActionResult GetConnectionSettings()
        {
            var settings = _configuration.GetSection("ConnectionStrings").Get<ConnectionSettings>();
            return Json(settings);
        }


        [HttpPost]
        public IActionResult SaveSettings(ConnectionSettings settings)
        {
            
            var json = System.IO.File.ReadAllText("appsettings.json");
            var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

           
            jsonObj.ConnectionStrings.IPAddress = settings.IPAddress;
            jsonObj.ConnectionStrings.Port = settings.Port;

           
            System.IO.File.WriteAllText("appsettings.json", Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented));

            return RedirectToAction("Index");
        }
    }
}
