using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WarehouseManagementSystem.Services;
using WarehouseManagementSystem.Models;
using IOFile = System.IO.File;
using System.IO;

namespace WarehouseManagementSystem.Controllers
{
    public class SystemManagementController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ISystemExpirationService _expirationService;

        public SystemManagementController(IConfiguration configuration, ISystemExpirationService expirationService)
            : base(expirationService)
        {
            _configuration = configuration;
            _expirationService = expirationService;
        }

        public IActionResult Index()
        {
            var model = new SystemManagementViewModel
            {
                ExpirationDate = _expirationService.GetExpirationDate(),
                RemainingDays = _expirationService.GetRemainingDays()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateExpirationDate(SystemManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // 读取配置文件
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = IOFile.ReadAllText(configPath);
                var jsonObj = JObject.Parse(json);

                // 更新到期日期
                if (jsonObj["SystemAccess"] == null)
                {
                    jsonObj["SystemAccess"] = new JObject();
                }

                jsonObj["SystemAccess"]["ExpirationDate"] = model.ExpirationDate?.ToString("yyyy-MM-dd");

                // 写回配置文件
                IOFile.WriteAllText(configPath, jsonObj.ToString());

                TempData["Message"] = "系统到期日期已更新！";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"更新失败: {ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index");
        }
    }
} 