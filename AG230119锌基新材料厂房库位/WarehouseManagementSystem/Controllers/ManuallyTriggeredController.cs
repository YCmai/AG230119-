using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Controllers
{
    public class ManuallyTriggeredController : Controller
    {

        private readonly ApplicationDbContext _context;

        private readonly ConnectionStatusService _connectionStatusService;

        private readonly HttpClient _httpClient;


        private readonly IConfiguration _configuration;

        public ManuallyTriggeredController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration)
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



        // GET: ManuallyTriggeredController
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

                        
                        var locationList = locations.Select(l => new { id = l.Id, text = l.Name }).ToList();
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


        public async Task<JsonResult> SubmitTask(string startLocationId)
        {
            if (!_connectionStatusService.IsConnected)
            {
                return Json(new { success = false, message = "提交任务时发生错误,服务器连接失败" });
            }

            try
            {
               
                var (baseUrl, port, http) = GetConnectionParameters();

               
                var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/SubmitTask";

                var response = await _httpClient.PostAsJsonAsync(apiUrl, startLocationId);
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


    }
}
