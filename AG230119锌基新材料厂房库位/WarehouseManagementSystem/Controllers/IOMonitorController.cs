using System.Net;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

using WarehouseManagementSystem.Models.IO;
using WarehouseManagementSystem.Service.Io;

namespace WarehouseManagementSystem.Controllers
{
    // Controllers/IOMonitorController.cs
    public class IOMonitorController : Controller
    {
        private readonly IIODeviceService _deviceService;
        private readonly IIOService _ioService;
        private readonly ILogger<IOMonitorController> _logger;
      

        public IOMonitorController(
            IIODeviceService deviceService,
            IIOService ioService,
            ILogger<IOMonitorController> logger
           )
        {
            _deviceService = deviceService;
            _ioService = ioService;
            _logger = logger;
           
        }

        // 页面入口
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.CurrentUser = "YCmai"; // 当前用户
                ViewBag.CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var devices = await _deviceService.GetAllDevicesAsync();
                return View(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备列表失败");
                return View("Error");
            }
        }

        // 新增任务列表页面动作方法
        public async Task<IActionResult> Tasks()
        {
            try
            {
                ViewBag.CurrentUser = "YCmai";
                ViewBag.CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var tasks = await _deviceService.GetRCS_IOAGV_TasksAsync();
                return View(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取IO任务列表失败");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestSignals()
        {
            try
            {
            var signals = await _deviceService.GetLatestSignalsAsync(); // 从数据库获取最新信号数据
            return Json(signals); // 返回JSON格式的数据
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新信号数据失败");
                return Json(new { success = false, message = "获取信号数据失败，请稍后再试。" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] RCS_IODevices device)
        {
            try
            {
                // 基本验证
                if (device == null)
                {
                    return Json(new { success = false, message = "设备数据不能为空" });
                }

                if (string.IsNullOrWhiteSpace(device.Name))
                {
                    return Json(new { success = false, message = "设备名称不能为空" });
                }

                if (string.IsNullOrWhiteSpace(device.IP))
                {
                    return Json(new { success = false, message = "设备IP地址不能为空" });
                }

                // 检查IP地址格式
                if (!IsValidIPAddress(device.IP))
                {
                    return Json(new { success = false, message = "无效的IP地址格式" });
                }

                // 检查设备名称是否已存在
                var existingDeviceWithName = await _deviceService.GetDeviceByNameAsync(device.Name);
                if (existingDeviceWithName != null)
                {
                    return Json(new { success = false, message = $"设备名称 '{device.Name}' 已存在" });
                }

                // 检查IP地址是否已存在
                var existingDeviceWithIP = await _deviceService.GetDeviceByIPAsync(device.IP);
                if (existingDeviceWithIP != null)
                {
                    return Json(new { success = false, message = $"IP地址 '{device.IP}' 已被使用" });
                }

                // 设置创建和更新时间
                device.CreatedTime = DateTime.Now;
                device.UpdatedTime = DateTime.Now;
               

                var newDevice = await _deviceService.AddDeviceAsync(device);
                _logger.LogInformation("成功添加设备: {@Device}", newDevice);

                return Json(new { success = true, data = newDevice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加设备失败: {@Device}", device);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSignal([FromBody] RCS_IOSignals signal)
        {
            try
            {
                // 基本验证
                if (signal == null)
                {
                    return Json(new { success = false, message = "信号数据不能为空" });
                }

                if (string.IsNullOrWhiteSpace(signal.Name))
                {
                    return Json(new { success = false, message = "信号名称不能为空" });
                }

                // 验证设备是否存在
                var device = await _deviceService.GetDeviceByIdAsync(signal.DeviceId);
                if (device == null)
                {
                    return Json(new { success = false, message = "指定的设备不存在" });
                }


                // 检查同一设备下是否存在相同名称的信号
                var existingSignalWithName = await _deviceService.GetSignalByNameAndDeviceAsync(signal.DeviceId, signal.Name);
                if (existingSignalWithName != null)
                {
                    return Json(new { success = false, message = $"该设备下已存在名称为 '{signal.Name}' 的信号" });
                }

                // 检查同一设备下是否存在相同地址的信号
                var existingSignalWithAddress = await _deviceService.GetSignalByAddressAndDeviceAsync(signal.DeviceId, signal.Address);
                if (existingSignalWithAddress != null)
                {
                    return Json(new { success = false, message = $"该设备下已存在地址为 {signal.Address} 的信号" });
                }

                // 设置创建和更新时间
                signal.CreatedTime = DateTime.Now;
                signal.UpdatedTime = DateTime.Now;
                signal.Value = 0;

                var signalId = await _deviceService.AddSignalAsync(signal);
                _logger.LogInformation("成功添加信号: {@Signal}", signal);

                return Json(new { success = true, data = signalId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加信号失败: {@Signal}", signal);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 辅助方法：验证IP地址格式
        private bool IsValidIPAddress(string ipAddress)
        {
            try
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // 尝试解析IP地址
            return System.Net.IPAddress.TryParse(ipAddress, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"验证IP地址格式失败: {ipAddress}");
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetIOTasks()
        {
            try
            {
                var tasks = await _deviceService.GetRCS_IOAGV_TasksAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取IO任务列表时发生错误");
                return StatusCode(500, new { success = false, message = "获取任务列表失败", error = ex.Message });
            }
        }




        [HttpPost]
        public async Task<IActionResult> WriteSignal([FromBody] WriteSignalRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "无效的请求数据" });
                }

                if (string.IsNullOrEmpty(request.IP))
                {
                    return Json(new { success = false, message = "IP地址不能为空" });
                }

                if (string.IsNullOrEmpty(request.Address))
                {
                    return Json(new { success = false, message = "地址不能为空" });
                }

                // 尝试解析地址字符串为枚举值
                if (!Enum.TryParse<EIOAddress>(request.Address, out EIOAddress addressEnum))
                {
                    return Json(new { success = false, message = $"无效的地址值: {request.Address}" });
                }

                // 检查是否是输入地址（DI）
                if (addressEnum.ToString().StartsWith("DI"))
                {
                    return Json(new { success = false, message = "DI地址为只读，不能写入" });
                }

                _logger.LogInformation("正在创建写入信号任务: IP={IP}, Address={Address}, Value={Value}",
                    request.IP, addressEnum, request.Value);

                // 创建IO任务而不是直接写入
                var taskId = await _ioService.AddIOTask(
                    taskType: "ArrivalNotify",
                    deviceIP: request.IP,
                    signalAddress: request.Address,
                    value: request.Value,
                    taskId: $"MANUAL_{DateTime.Now:yyyyMMddHHmmssfff}"
                );

                if (taskId > 0)
                {
                    _logger.LogInformation("信号写入任务创建成功: TaskId={TaskId}, User={User}", taskId, "YCmai");
                    return Json(new { success = true, taskId = taskId });
                }
                else
                {
                    throw new Exception("创建任务失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建写入信号任务失败: IP={IP}, Address={Address}",
                    request?.IP, request?.Address);
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// 读取信号状态
        /// </summary>
        /// <param name="ip">设备IP地址</param>
        /// <param name="address">信号地址(如：DI1、DO1等)</param>
        [HttpGet]
        public async Task<IActionResult> ReadSignal(string ip, string address)
        {
            try
            {
                if (string.IsNullOrEmpty(ip))
                {
                    return Json(new { success = false, message = "IP地址不能为空" });
                }

                if (string.IsNullOrEmpty(address))
                {
                    return Json(new { success = false, message = "地址不能为空" });
                }

                // 尝试解析地址字符串为枚举值
                if (!Enum.TryParse<EIOAddress>(address, out EIOAddress addressEnum))
                {
                    _logger.LogWarning("无效的信号地址: IP={IP}, Address={Address}, User={User}, Time={Time}",
                        ip, address, "YCmai", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    return Json(new { success = false, message = $"无效的地址值: {address}" });
                }

                _logger.LogInformation("正在读取信号: IP={IP}, Address={Address}, User={User}, Time={Time}",
                    ip, addressEnum, "YCmai", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                var value = await _ioService.ReadSignal(ip, addressEnum);

                _logger.LogInformation("信号读取成功: IP={IP}, Address={Address}, Value={Value}, User={User}, Time={Time}",
                    ip, addressEnum, value, "YCmai", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                return Json(new
                {
                    success = true,
                    value,
                    address = addressEnum.ToString(),
                    hexAddress = $"0x{((int)addressEnum).ToString("X4")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取信号失败: IP={IP}, Address={Address}, User={User}, Time={Time}",
                    ip, address, "YCmai", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                return Json(new
                {
                    success = false,
                    message = $"读取信号失败: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSignal([FromBody] ToggleDeviceRequest request)
        {
            try
            {
                await _deviceService.DeleteSignAsync(request.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除信号失败");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDevice([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "无效的设备ID" });
                }

                _logger.LogInformation("正在删除设备: ID={Id}", id);

                var device = await _deviceService.GetDeviceByIdAsync(id);
                if (device == null)
                {
                    return Json(new { success = false, message = "设备不存在" });
                }

                // 删除设备及其关联的信号
                await _deviceService.DeleteDeviceAsync(id);

                _logger.LogInformation("设备删除成功: ID={Id}", id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备失败: ID={Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDevice([FromBody] RCS_IODevices device)
        {
            try
            {
                device.UpdatedTime = DateTime.Now;
                await _deviceService.UpdateDeviceAsync(device);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备失败");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDevice([FromBody] ToggleDeviceRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "无效的请求数据" });
                }

                _logger.LogInformation("正在切换设备状态: ID={Id}, IsEnabled={IsEnabled}",
                    request.Id, request.IsEnabled);

                var device = await _deviceService.GetDeviceByIdAsync(request.Id);
                if (device == null)
                {
                    return Json(new { success = false, message = "设备不存在" });
                }

                device.IsEnabled = request.IsEnabled;
                device.UpdatedTime = DateTime.Now;
                await _deviceService.UpdateDeviceAsync(device);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换设备状态失败: ID={Id}", request?.Id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddIOTask([FromBody] RCS_IOAGV_Tasks task)
        {
            try
            {
                if (task == null)
                {
                    return Json(new { success = false, message = "任务数据不能为空" });
                }

                // 创建IO任务
                var taskId = await _ioService.AddIOTask(
                    taskType: task.TaskType,
                    deviceIP: task.DeviceIP,
                    signalAddress: task.SignalAddress,
                    value: task.Value,
                    taskId: task.TaskId
                );

                if (taskId > 0)
                {
                    _logger.LogInformation("成功创建IO任务: {@Task}", task);
                    return Json(new { success = true, data = taskId });
                }
                else
                {
                    throw new Exception("创建任务失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建IO任务失败: {@Task}", task);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class ToggleDeviceRequest
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }
}
