using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;
using WarehouseManagementSystem.Models;
using Newtonsoft.Json;

namespace WarehouseManagementSystem.Controllers
{
    public class EmptyTrayStatusModificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ConnectionStatusService _connectionStatusService;

        private readonly HttpClient _httpClient;


        private readonly IConfiguration _configuration;


        public EmptyTrayStatusModificationController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration)
        {
            _context = context;
            _connectionStatusService = connectionStatusService;
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        private (string baseUrl, string port, string http) GetConnectionParameters()
        {
            var baseUrl = _configuration["ConnectionStrings:IPAddress"];
            var port = _configuration["ConnectionStrings:Port"];
            var http = _configuration["ConnectionStrings:Http"];
            return (baseUrl, port, http);
        }


        // 列表展示和搜索
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10; // 每页显示10条数据

            // 提取连接参数
            var (baseUrl, port, http) = GetConnectionParameters();


            if (_connectionStatusService.IsConnected)
            {

                var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetEmptyLocation?page={page}&pageSize={pageSize}";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {

                    return View(new PagedResult<RCS_Locations>
                    {
                        TotalItems = 0,
                        Items = new List<RCS_Locations>(),
                        TotalPages = 0
                    });
                }

                // 解析 API 返回的结果
                var result = await response.Content.ReadAsStringAsync();
                var pagedResult = System.Text.Json.JsonSerializer.Deserialize<PagedResult<RCS_Locations>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(pagedResult);
            }
            else
            {

                return View(new PagedResult<RCS_Locations>
                {
                    TotalItems = 0,
                    Items = new List<RCS_Locations>(),
                    PageNumber = page,
                    TotalPages = 0
                });
            }

        }


        public async Task<IActionResult> ResetMaterialCode(int id)
        {
            // 提取连接参数
            var (baseUrl, port, http) = GetConnectionParameters();

            if (_connectionStatusService.IsConnected)
            {
                var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/ResetMaterialCode?id={id}";

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, null); // 发送 POST 请求，无需请求体

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "重置失败，请稍后重试。" });
                }

                return Json(new { success = true, message = "重置成功。" });
            }
            else
            {
                return Json(new { success = false, message = "未建立连接。" });
            }
        }







        // GET: TasksController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TasksController/Create
        public ActionResult Create()
        {
            return View();
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
            catch
            {
                return View();
            }
        }

        // GET: TasksController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
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
            catch
            {
                return View();
            }
        }

        // GET: TasksController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
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
            catch
            {
                return View();
            }
        }
    }

}
