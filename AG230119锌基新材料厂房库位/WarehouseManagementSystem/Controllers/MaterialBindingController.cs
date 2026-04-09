using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;

public class MaterialBindingController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly ConnectionStatusService _connectionStatusService;

    private readonly HttpClient _httpClient;


    private readonly IConfiguration _configuration;

    public MaterialBindingController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration)
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




    public async Task<IActionResult> Index(string searchString, int page = 1)
    {
        int pageSize = 10; // 每页显示10条数据

        // 提取连接参数
        var (baseUrl, port, http) = GetConnectionParameters();


        if (_connectionStatusService.IsConnected)
        {
         
            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterialBinding?searchString={searchString}&page={page}&pageSize={pageSize}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
              
                return View(new PagedResult<RCS_MaterialBinding>
                {
                    TotalItems = 0,
                    Items = new List<RCS_MaterialBinding>(), 
                    TotalPages = 0  
                });
            }

            // 解析 API 返回的结果
            var result = await response.Content.ReadAsStringAsync();
            var pagedResult = System.Text.Json.JsonSerializer.Deserialize<PagedResult<RCS_MaterialBinding>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(pagedResult);
        }
        else
        {
            
            return View(new PagedResult<RCS_MaterialBinding>
            {
                TotalItems = 0,
                Items = new List<RCS_MaterialBinding>(), 
                PageNumber = page,
                TotalPages = 0 
            });
        }
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        var (baseUrl, port, http) = GetConnectionParameters();

        try
        {
            if (!_connectionStatusService.IsConnected)
            {
                return View(new RCS_MaterialBinding());
            }

            var apiurli = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterialCodes";

            var response2 = await _httpClient.GetAsync(apiurli);

            if (response2.IsSuccessStatusCode)
            {
                var materialCodesJson = await response2.Content.ReadAsStringAsync(); 
               
            }

          

            if (id == null)
            {
                return View(new RCS_MaterialBinding());
            }
            else
            {
                var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterialBindingModel?id={id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var result = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var materialBinding = System.Text.Json.JsonSerializer.Deserialize<RCS_MaterialBinding>(result, options);
                
                // 确保从后端传回来的数据包含 MaterialCode
                return View(materialBinding);
            }
        }
        catch (Exception ex)
        {

            TempData["Message"] = "获取物料信息失败！请稍后再试。";
            TempData["MessageType"] = "danger";
            return View(new RCS_MaterialBinding());
        }
    }





    // 获取MaterialDesc和RemainingQty
    public async Task<IActionResult> GetMaterialDetails(string materialCode)
    {
        var (baseUrl, port, http) = GetConnectionParameters();

        try
        {
            if (!_connectionStatusService.IsConnected)
            {
                return Json(new { success = false, message = "无法连接到服务器." });
            }

            if (string.IsNullOrEmpty(materialCode))
            {
                return Json(new { success = false, message = "物料代码为必填项." });
            }

            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterialDetails?materialCode={materialCode}";

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var materialDetailsJson = await response.Content.ReadAsStringAsync(); // Assume the API returns an object with MaterialDesc and RemainingQty
                var materialDetails = JsonConvert.DeserializeObject<MaterialDetails>(materialDetailsJson);
                return Json(new { success = true, materialDetails });
            }
            else
            {
                return Json(new { success = false, message = "检索物料详细信息失败." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "检索物料详细信息失败，请稍后重试." });
        }
    }


    public async Task<IActionResult> GetMaterialCodes()
    {
        var (baseUrl, port, http) = GetConnectionParameters();

        try
        {
            if (!_connectionStatusService.IsConnected)
            {
                return Json(new { success = false, message = "Failed to connect to the server." });
            }

            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterialCodes";

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var materialCodesJson = await response.Content.ReadAsStringAsync(); // Assuming the response is a JSON array
                var materialCodes = "";

                return Json(new { success = true, materialCodes });
            }
            else
            {
                return Json(new { success = false, message = "检索物料代码失败." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "检索物料代码失败，请稍后重试." });
        }
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(RCS_MaterialBinding location)
    {
        var (baseUrl, port, http) = GetConnectionParameters();

        try
        {
            if (!_connectionStatusService.IsConnected)
            {
                TempData["Message"] = "保存失败，服务器连接失败";
                TempData["MessageType"] = "danger";
                return View(location);
            }


            if (string.IsNullOrEmpty(location.MaterialCode))
            {
                TempData["Message"] = "materialCode不为空！";
                TempData["MessageType"] = "danger";
                return View(location);
            }


            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/CreateMaterialBinding";
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, location);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = location.Id == 0 ? "创建存储库成功" : "数据库材料已成功更新！";
                TempData["MessageType"] = "success";
                TempData["RedirectAfterDelay"] = true; // 标记是否需要延迟跳转
                return View(location); // 返回当前页面以显示提示信息
            }

            TempData["Message"] = "重复的产品代码无法保存！";
            TempData["MessageType"] = "danger";
            return View(location);
        }
        catch (Exception ex)
        {
            TempData["Message"] = "操作失败。请稍后再试。";
            TempData["MessageType"] = "danger";
            return View(location);
        }
    }


    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (baseUrl, port, http) = GetConnectionParameters();

        try
        {
            if (!_connectionStatusService.IsConnected)
            {
                return Json(new { success = false, message = "删除或连接到服务器失败。" });
            }

            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/MatBindDeleteConfirmed?id={id}";

            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "数据库位已成功删除。！" });
            }
            else
            {
                return Json(new { success = false, message = "删除失败，找不到数据库材料代码。" });
            }
        }
        catch (Exception ex)
        {
           
            return Json(new { success = false, message = "删除失败，请稍后重试。" });
        }
    }

   

}


public class MaterialDetails
{
    public string MaterialDesc { get; set; }
    public int RemainingQty { get; set; }
}
