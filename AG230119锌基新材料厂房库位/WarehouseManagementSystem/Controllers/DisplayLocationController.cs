using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;
using Dapper;
using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Controllers;
using WarehouseManagementSystem.Services;
using System.Text.RegularExpressions;

public class DisplayLocationController : BaseController
{
    private readonly ILocationService _locationService;
    private readonly ILogger<DisplayLocationController> _logger;

    public DisplayLocationController(ILocationService locationService, ILogger<DisplayLocationController> logger, ISystemExpirationService expirationService)
        : base(expirationService)
    {
        _locationService = locationService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string searchString, int page = 1)
    {
        try
        {
            int pageSize = 5000;
            var (items, totalItems) = await _locationService.GetLocations(searchString, page, pageSize);
            var (available, used) = await _locationService.GetStorageCapacityStats();

            ViewData["StorageCapacityAvailable"] = available;
            ViewData["StorageCapacityUse"] = used;

            // 分组自然排序
            var groupList = items
                .Select(l => l.Group)
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => Regex.Matches(g, "\\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToList(), new ListIntComparer())
                .ToList();
            ViewData["GroupList"] = groupList.ToArray();

            return View(new PagedResult<RCS_Locations>
            {
                Items = items.ToList(),
                TotalItems = totalItems,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位列表失败");
            return View(new PagedResult<RCS_Locations>());
        }
    }

    // 数字自然排序比较器
    public class ListIntComparer : IComparer<List<int>>
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
    
    // 修改批量操作方法，接受储位ID列表而不仅仅是区域
    [HttpPost]
    public async Task<IActionResult> BatchClearMaterials(List<int> locationIds)
    {
        try
        {
            if (locationIds == null || !locationIds.Any())
            {
                return Json(new { success = false, message = "请选择要操作的储位" });
            }
            
            var (success, message, affectedCount) = await _locationService.BatchClearMaterialsByIds(locationIds);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量清空储位物料失败");
            return Json(new { success = false, message = "批量操作失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchToggleLock(List<int> locationIds, bool lockState)
    {
        try
        {
            if (locationIds == null || !locationIds.Any())
            {
                return Json(new { success = false, message = "请选择要操作的储位" });
            }
            
            var (success, message, affectedCount) = await _locationService.BatchToggleLockByIds(locationIds, lockState);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            string operation = lockState ? "锁定" : "解锁";
            _logger.LogError(ex, $"批量{operation}储位失败");
            return Json(new { success = false, message = $"批量{operation}失败，请稍后再试。" });
        }
    }

    // 保留原有的按区域批量操作方法
    [HttpPost]
    public async Task<IActionResult> BatchClearMaterialsByGroup(string group)
    {
        try
        {
            if (string.IsNullOrEmpty(group))
            {
                return Json(new { success = false, message = "请指定要操作的区域" });
            }
            
            var (success, message, affectedCount) = await _locationService.BatchClearMaterials(group);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量清空区域 {group} 的物料失败");
            return Json(new { success = false, message = "批量操作失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchToggleLockByGroup(string group, bool lockState)
    {
        try
        {
            if (string.IsNullOrEmpty(group))
            {
                return Json(new { success = false, message = "请指定要操作的区域" });
            }
            
            var (success, message, affectedCount) = await _locationService.BatchToggleLock(group, lockState);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            string operation = lockState ? "锁定" : "解锁";
            _logger.LogError(ex, $"批量{operation}区域 {group} 的储位失败");
            return Json(new { success = false, message = $"批量{operation}失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchSetQuantity(string group, bool isSetFull)
    {
        try
        {
            if (string.IsNullOrEmpty(group))
            {
                return Json(new { success = false, message = "请指定要操作的区域" });
            }
            // 这里假设满为100，空为0，如有不同请自行调整
            var targetQuantity = isSetFull ? "满" : "0";
            var (success, message, affectedCount) = await _locationService.BatchSetQuantityByGroup(group, targetQuantity);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量设置区域 {group} 的数量失败");
            return Json(new { success = false, message = "批量操作失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchSetQuantitySelected(List<int> locationIds, bool isSetFull)
    {
        try
        {
            if (locationIds == null || !locationIds.Any())
            {
                return Json(new { success = false, message = "请选择要操作的储位" });
            }
            var targetQuantity = isSetFull ? "满" : "0";
            var (success, message, affectedCount) = await _locationService.BatchSetQuantityByIds(locationIds, targetQuantity);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量设置储位数量失败");
            return Json(new { success = false, message = "批量操作失败，请稍后再试。" });
        }
    }




    [HttpPost]
    public async Task<IActionResult> BatchUpdateMaterialCode(List<int> locationIds, string newMaterialCode)
    {
        try
        {
            if (locationIds == null || !locationIds.Any())
            {
                return Json(new { success = false, message = "请选择要操作的储位" });
            }
            
            if (string.IsNullOrEmpty(newMaterialCode))
            {
                return Json(new { success = false, message = "请输入新的物料编号" });
            }
            
            var (success, message, affectedCount) = await _locationService.BatchUpdateMaterialCode(locationIds, newMaterialCode);
            return Json(new { success, message, affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量修改物料编号失败");
            return Json(new { success = false, message = "批量修改失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchUpdateMaterialCodeByGroup(string group, string newMaterialCode)
    {
        try
        {
            if (string.IsNullOrEmpty(group))
            {
                return Json(new { success = false, message = "请选择要操作的分组" });
            }
            
            if (string.IsNullOrEmpty(newMaterialCode))
            {
                return Json(new { success = false, message = "请输入新的物料编号" });
            }
            
            // 获取该分组内的所有储位ID
            var locations = await _locationService.GetLocationsByGroup(group);
            var locationIds = locations.Select(l => l.Id).ToList();
            
            if (!locationIds.Any())
            {
                return Json(new { success = false, message = "所选分组内没有储位" });
            }
            
            var result = await _locationService.BatchUpdateMaterialCode(locationIds, newMaterialCode);
            return Json(new { success = result.success, message = result.message, affectedCount = result.affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量修改分组 {group} 的物料编号失败");
            return Json(new { success = false, message = "批量修改失败，请稍后再试。" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BatchClearMaterialCodeByGroup(string group)
    {
        try
        {
            if (string.IsNullOrEmpty(group))
            {
                return Json(new { success = false, message = "请选择要操作的分组" });
            }
            
            var result = await _locationService.BatchClearMaterialCodeByGroup(group);
            return Json(new { success = result.success, message = result.message, affectedCount = result.affectedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量清空分组 {group} 的物料编号失败");
            return Json(new { success = false, message = "批量清空物料编号失败，请稍后再试。" });
        }
    }
}
