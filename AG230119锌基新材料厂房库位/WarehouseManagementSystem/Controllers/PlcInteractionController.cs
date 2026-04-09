using Microsoft.AspNetCore.Mvc;

namespace WarehouseManagementSystem.Controllers
{
    public class PlcInteractionController : Controller
    {
        private readonly IPlcService _plcService;
        private readonly ILogger<PlcInteractionController> _logger;

        public PlcInteractionController(IPlcService plcService, ILogger<PlcInteractionController> logger)
        {
            _plcService = plcService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
               

                var result = await _plcService.GetPlcInteractionsAsync(
                    pageNumber, pageSize);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取PLC交互记录失败");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _plcService.DeletePlcInteractionAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除PLC交互记录失败");
                return Json(new { success = false, message = "删除失败，请重试" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            try
            {
                await _plcService.ClearPlcInteractionsAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空PLC交互记录失败");
                return Json(new { success = false, message = "清空失败，请重试" });
            }
        }
    }
}
