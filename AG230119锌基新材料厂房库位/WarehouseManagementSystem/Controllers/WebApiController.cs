using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Service.WepApi;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Controllers
{
    public class WebApiController : BaseController
    {
        private readonly IWebApiTaskService _taskService;

        public WebApiController(IWebApiTaskService taskService, ISystemExpirationService expirationService)
            : base(expirationService)
        {
            _taskService = taskService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _taskService.GetPagedTasksAsync(page, pageSize);
            return View(result);
        }
    }
}
