using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using OfficeOpenXml;

using WarehouseManagementSystem.Models;
using Microsoft.Extensions.Logging;

namespace WarehouseManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ConnectionStatusService _connectionStatusService;

        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration, ITaskService taskService, ILogger<TasksController> logger)
        {
            _context = context;
            _connectionStatusService = connectionStatusService;
            _httpClient = new HttpClient();
            _configuration = configuration;
            _taskService = taskService;
            _logger = logger;
        }

        private (string baseUrl, string port, string http) GetConnectionParameters()
        {
            try
            {
                var baseUrl = _configuration["ConnectionStrings:IPAddress"];
                var port = _configuration["ConnectionStrings:Port"];
                var http = _configuration["ConnectionStrings:Http"];
                return (baseUrl, port, http);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取连接参数失败");
                return (string.Empty, string.Empty, string.Empty);
            }
        }

        public async Task<IActionResult> Index(int page = 1, string dropLocation = "",
            DateTime? filterDate = null, DateTime? endDate = null, string palletId = "", int pageSize = 10)
        {
            try
            {
                var (items, totalItems) = await _taskService.GetUserTasks(page, pageSize, filterDate, endDate);

                // 保存筛选条件到 ViewData
                ViewData["dropLocation"] = dropLocation;
                ViewData["filterDate"] = filterDate?.ToString("yyyy-MM-dd");
                ViewData["endDate"] = endDate?.ToString("yyyy-MM-dd");
                ViewData["palletId"] = palletId;

                return View(new PagedResult<RCS_UserTasks>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务列表失败");
                return View(new PagedResult<RCS_UserTasks>
                {
                    Items = new List<RCS_UserTasks>(),
                    TotalItems = 0,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = 0
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelTask(int id)
        {
            try
            {
                var (success, message) = await _taskService.CancelTask(id);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取消任务 {id} 失败");
                return Json(new { success = false, message = "取消任务失败，请稍后再试。" });
            }
        }

        // GET: TasksController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查看任务详情 {id} 失败");
                return View("Error");
            }
        }

        // GET: TasksController/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务页面加载失败");
                return View("Error");
            }
        }

        // POST: TasksController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务失败");
                return View();
            }
        }

        // GET: TasksController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"编辑任务 {id} 页面加载失败");
                return View("Error");
            }
        }

        // POST: TasksController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"编辑任务 {id} 失败");
                return View();
            }
        }

        // GET: TasksController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除任务 {id} 页面加载失败");
                return View("Error");
            }
        }

        // POST: TasksController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除任务 {id} 失败");
                return View();
            }
        }
    }
}
