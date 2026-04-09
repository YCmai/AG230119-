using Microsoft.AspNetCore.Mvc;

using WarehouseManagementSystem.Models.PLC;

namespace WarehouseManagementSystem.Controllers
{
    public class PlcMonitorController : Controller
    {
        private readonly IPlcService _plcService;
        private readonly ILogger<PlcMonitorController> _logger;

        public PlcMonitorController(IPlcService plcService, ILogger<PlcMonitorController> logger)
        {
            _plcService = plcService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // 不再获取和处理数据，只返回视图
                // 页面将通过AJAX加载所有数据
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取PLC状态失败");
                return View("Error");
            }
        }

        private string GetDisplayValue(string name, string address, string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            bool isPlatingLine = name.Contains("电镀线");
            bool isAssemblyLine = name.Contains("总装线");

            // 电镀线处理逻辑
            if (isPlatingLine)
            {
                // DM31000和DM32000直接返回原值
                if (address == "DM31000" || address == "DM32000")
                    return value;

                // 获取数字后缀
                string suffix = value.Length > 1 ? value.Substring(1) : "";

                // PLC写入寄存器（DM30100, DM30300, DM30500, DM30700）
                if (address.EndsWith("100") || address.EndsWith("300") ||
                    address.EndsWith("500") || address.EndsWith("700"))
                {
                    switch (value)
                    {
                        case var x when x.StartsWith("X"): return $"PLC呼叫工位AGV ({value})";
                        case var n when n.StartsWith("N"): return $"PLC允许AGV进入 ({value})";
                        case var f when f.StartsWith("F"): return $"允许AGV放料 ({value})";
                        case var t when t.StartsWith("T"): return $"AGV允许离开 ({value})";
                        default: return value;
                    }
                }

                // AGV写入寄存器（DM30200, DM30400, DM30600, DM30800）
                if (address.EndsWith("200") || address.EndsWith("400") ||
                    address.EndsWith("600") || address.EndsWith("800"))
                {
                    switch (value)
                    {
                        case var y when y.StartsWith("Y"): return $"AGV应答回应 ({value})";
                        case var ng when ng.StartsWith("NG"): return $"应答失败 ({value})";
                        case var m when m.StartsWith("M"): return $"AGV到达工位外围工位 ({value})";
                        case var e when e.StartsWith("E"): return $"AGV到达指定工位 ({value})";
                        case var k when k.StartsWith("K"): return $"AGV放料完成 ({value})";
                        default: return value;
                    }
                }
            }

            // 总装线处理逻辑
            if (isAssemblyLine)
            {
                // DM30300和DM30600直接返回原值
                if (address == "DM30300" || address == "DM30600")
                    return value;

                // PLC写入寄存器（DM30100, DM30400, DM30700）
                if (address.EndsWith("100") || address.EndsWith("400") || address.EndsWith("700"))
                {
                    switch (value)
                    {
                        case var x when x.StartsWith("X"): return $"PLC呼叫工位AGV ({value})";
                        case var n when n.StartsWith("N"): return $"PLC允许AGV进入 ({value})";
                        case var f when f.StartsWith("F"): return $"允许AGV放料 ({value})";
                        case var t when t.StartsWith("T"): return $"AGV允许离开 ({value})";
                        default: return value;
                    }
                }

                // AGV写入寄存器（DM30200, DM30500, DM30800）
                if (address.EndsWith("200") || address.EndsWith("500") || address.EndsWith("800"))
                {
                    switch (value)
                    {
                        case var y when y.StartsWith("Y"): return $"AGV应答回应 ({value})";
                        case var ng when ng.StartsWith("NG"): return $"应答失败 ({value})";
                        case var m when m.StartsWith("M"): return $"AGV到达工位外围工位 ({value})";
                        case var e when e.StartsWith("E"): return $"AGV到达指定工位 ({value})";
                        case var k when k.StartsWith("K"): return $"AGV放料完成 ({value})";
                        default: return value;
                    }
                }
            }

            return value;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestData(int pageNumber = 1, int pageSize = 10, string groupName = "")
        {
            try
            {
                // 使用正确的参数调用 GetPlcAddressesAsync
                var addresses = await _plcService.GetPlcAddressesAsync(pageNumber, pageSize, groupName);

                // 处理返回的数据
                var groupedData = addresses.Items
                    .GroupBy(x => x.Name.Split('-')[0])
                    .Select(g => new {
                        groupName = g.Key,
                        items = g.Select(x => new {
                            id = x.Id,
                            name = x.Name,
                            address = x.Address,
                            currentValue = GetDisplayValue(x.Name, x.Address, x.CurrentValue),
                            stationNumber = x.StationNumber,
                            updateTime = x.UpdateTime?.ToString("HH:mm:ss")
                        }).ToList()
                    }).ToList();

                return Json(new
                {
                    success = true,
                    data = groupedData,
                    pagination = new
                    {
                        pageNumber = addresses.PageNumber,
                        pageSize = addresses.PageSize,
                        totalPages = addresses.TotalPages,
                        totalItems = addresses.TotalItems,
                        hasNextPage = addresses.HasNextPage,
                        hasPreviousPage = addresses.HasPreviousPage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取PLC最新状态失败");
                return Json(new { success = false, message = "获取数据失败" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> WriteValue(int id, string value)
        {
            try
            {
                var result = await _plcService.WriteValueAsync(id, value, "system", "system");
                return Json(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入PLC值失败");
                return Json(new { success = false, message = "写入失败，请重试" });
            }
        }

       
    }
}
