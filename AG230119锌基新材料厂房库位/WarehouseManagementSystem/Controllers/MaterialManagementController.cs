using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;

public class MaterialManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly ConnectionStatusService _connectionStatusService;

    private readonly HttpClient _httpClient;


    private readonly IConfiguration _configuration;

    public MaterialManagementController(ApplicationDbContext context, ConnectionStatusService connectionStatusService, IConfiguration configuration)
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




    public async Task<IActionResult> Index(string materialCode, string palletID, int page = 1)
    {
        int pageSize = 10; // 每页显示10条数据

        // 提取连接参数
        var (baseUrl, port, http) = GetConnectionParameters();

        if (_connectionStatusService.IsConnected)
        {
            // 构建 API URL，包括多个搜索参数
            var apiUrl = $"{http}{baseUrl}:{port}/api/WarehouseSystem/GetMaterials?materialCode={materialCode}&palletID={palletID}&page={page}&pageSize={pageSize}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                // 返回一个空的 PagedResult 对象
                return View(new PagedResult<RCS_Locations>
                {
                    TotalItems = 0,
                    Items = new List<RCS_Locations>(),
                    TotalPages = 0
                });
            }

            // 解析 API 返回的结果
            var result = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonSerializer.Deserialize<PagedResult<RCS_Locations>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(pagedResult);
        }
        else
        {
            // 如果未连接服务器，返回一个空的 PagedResult 对象
            return View(new PagedResult<RCS_Locations>
            {
                TotalItems = 0,
                Items = new List<RCS_Locations>(),
                PageNumber = page,
                TotalPages = 0
            });
        }
    }


}
