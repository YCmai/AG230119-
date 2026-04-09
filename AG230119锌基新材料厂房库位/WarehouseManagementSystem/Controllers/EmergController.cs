using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models;

using static WarehouseManagementSystem.Controllers.ManuallyTriggeredController;

namespace WarehouseManagementSystem.Controllers
{
    public class EmergController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ConnectionStatusService _connectionStatusService;

        private readonly HttpClient _httpClient;


        private readonly IConfiguration _configuration;

        public EmergController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration)
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


        // GET: EmergController
        public async Task<ActionResult> Index()
        {
            if (_connectionStatusService.IsConnected)
            {
                try
                {
                    
                    var (baseUrl, port, http) = GetConnectionParameters();



                    
                    var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetLocations";

                    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {

                       
                        var result = await response.Content.ReadAsStringAsync();
                        var locations = JsonSerializer.Deserialize<List<RCS_Locations>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var locationList = locations.Select(l => new { id = l.Id, text = l.NodeRemark }).ToList();
                        ViewBag.Locations = Newtonsoft.Json.JsonConvert.SerializeObject(locationList);
                    }
                    else
                    {
                        
                        TempData["Message"] = "获取库位信息失败，请稍后重试。";
                    }
                }
                catch (Exception ex)
                {
                   
                    TempData["Message"] = "获取库位信息时发生错误：" + ex.Message;
                }
            }

            return View();
        }



        /// <summary>
        /// 1、空笼搬运，2满笼搬运
        /// </summary>
        /// <param name="startLocationId"></param>
        /// <param name="endLocationId"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<JsonResult> SubmitTask(string startLocationId, string endLocationId)
        {

            if (!_connectionStatusService.IsConnected)
            {
                return Json(new { success = false, message = "提交任务时发生错误,服务器连接失败" });
            }

            try
            {
               
                var (baseUrl, port, http) = GetConnectionParameters();

               
                var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/EmSubmitTask";

                var para=   new SubmitTaskRequest
                {
                    EndLocationId = endLocationId,
                    StartLocationId = startLocationId,
                    TaskType = "0",
                };
                var response = await _httpClient.PostAsJsonAsync(apiUrl, para);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    var jsonResult = JsonSerializer.Deserialize<SubmitTaskResult>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Json(new { success = jsonResult.Success, message = jsonResult.Message });
                }
                else
                {
                    return Json(new { success = false, message = "任务下发失败。" });
                }
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "提交任务时发生错误：" + ex.Message });
            }

        }

        

      

      

        // GET: EmergController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EmergController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmergController/Create
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

        // GET: EmergController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EmergController/Edit/5
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

        // GET: EmergController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EmergController/Delete/5
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

    public class SubmitTaskRequest
    {
        public string StartLocationId { get; set; }
        public string EndLocationId { get; set; }
        public string TaskType { get; set; }
    }

}
