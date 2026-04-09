using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;
using WarehouseManagementSystem.Controllers;
using WarehouseManagementSystem.Services;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static DisplayLocationController;

public class LocationController : BaseController
{
    private readonly ApplicationDbContext _context;

    private readonly ConnectionStatusService _connectionStatusService;

    private readonly HttpClient _httpClient;

    private readonly ILocationService _locationService;

    private readonly IConfiguration _configuration;

    private readonly ILogger<LocationController> _logger;

    public LocationController(ApplicationDbContext context, ILogger<LocationController> logger, 
        ILocationService locationService, ConnectionStatusService connectionStatusService, 
        IConfiguration configuration, ISystemExpirationService expirationService)
        : base(expirationService)
    {
        _context = context;
        _connectionStatusService = connectionStatusService;
        _httpClient = new HttpClient();
        _configuration = configuration;
        _locationService = locationService;
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

    public async Task<IActionResult> Index(string searchString, int page = 1)
    {
        try
        {
            int pageSize = 20; // 或者其他适合的页面大小
            ViewData["searchString"] = searchString;

            var (items, totalItems) = await _locationService.GetSearchLocations(searchString, page, pageSize);

            var model = new PagedResult<RCS_Locations>
            {
                Items = items.ToList(),
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位列表失败");
            return View(new PagedResult<RCS_Locations>());
        }
    }

    private int GetNodeRemarkPart(string nodeRemark, int index)
    {
        if (string.IsNullOrEmpty(nodeRemark)) return 0;
        var parts = nodeRemark.Split('-');
        if (index < parts.Length && int.TryParse(parts[index], out var n))
            return n;
        return 0;
    }

    // 分段数字自然排序Key
    private List<int> GetSegmentedIntList(string input)
    {
        if (string.IsNullOrEmpty(input)) return new List<int> { 0 };
        
        // 使用正则表达式提取数字部分
        var numbers = input.Split('-')
            .Select(s => {
                if (int.TryParse(s, out var n))
                    return n;
                // 如果解析失败，尝试提取数字部分
                var match = Regex.Match(s, @"\d+");
                return match.Success ? int.Parse(match.Value) : 0;
            })
            .ToList();
            
        return numbers;
    }

    // List<int> 比较器
    public class ListIntArrayComparer : IComparer<List<int>>
    {
        public int Compare(List<int> x, List<int> y)
        {
            int minLen = Math.Min(x.Count, y.Count);
            for (int i = 0; i < minLen; i++)
            {
                int cmp = x[i].CompareTo(y[i]);
                if (cmp != 0) return cmp;
            }
            return x.Count.CompareTo(y.Count);
        }
    }

    public async Task<IActionResult> CreateEdit(int? id)
    {
        try
        {
            if (id == null)
            {
                return View(new RCS_Locations());
            }

            var location = await _locationService.GetLocationById(id.Value);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位信息失败");
            TempData["Message"] = "获取库位信息失败！请稍后重试。";
            TempData["MessageType"] = "danger";
            return View(new RCS_Locations());
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEdit(RCS_Locations location)
    {
        try
        {
            var (success, message) = await _locationService.CreateOrUpdateLocation(location);

            TempData["Message"] = message;
            TempData["MessageType"] = success ? "success" : "danger";
            if (success)
            {
                TempData["RedirectAfterDelay"] = true;
            }

            return View(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存库位信息失败");
            TempData["Message"] = "保存失败，请稍后再试。";
            TempData["MessageType"] = "danger";
            return View(location);
        }
    }


    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id, int type)
    {
        try
        {
            var (success, message) = await _locationService.HandleLocationOperation(id, type);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作失败");
            return Json(new { success = false, message = "操作失败，请稍后再试。" });
        }
    }

    private void HandleOptionalFields(RCS_Locations location)
    {
        try
        {
            // 手动移除可选字段的错误
            if (string.IsNullOrEmpty(location.MaterialCode))
            {
                ModelState.Remove(nameof(location.MaterialCode));
            }
            if (string.IsNullOrEmpty(location.PalletID))
            {
                ModelState.Remove(nameof(location.PalletID));
            }
            if (string.IsNullOrEmpty(location.Weight))
            {
                ModelState.Remove(nameof(location.Weight));
            }
            if (string.IsNullOrEmpty(location.Quanitity))
            {
                ModelState.Remove(nameof(location.Quanitity));
            }
            if (string.IsNullOrEmpty(location.EntryDate))
            {
                ModelState.Remove(nameof(location.EntryDate));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理可选字段时发生错误");
        }
    }


}
