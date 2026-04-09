using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Dapper;
using WarehouseManagementSystem.Service.Io;
using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Models.IO;
using System.Collections.Generic;
using NPOI.XSSF.UserModel;
using System;

namespace WarehouseManagementSystem.Service.Tasks
{
    public class AGVTaskGenerationService : BackgroundService
    {
        private readonly ILogger<AGVTaskGenerationService> _logger;
        private readonly IDatabaseService _db;
        private readonly ITaskGenerationService _taskGenerationService;
        private ConcurrentDictionary<string, DateTime> _di7SignalStartTimes = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentDictionary<string, DateTime> _downlineSignalStartTimes = new ConcurrentDictionary<string, DateTime>();

        public AGVTaskGenerationService(
            ILogger<AGVTaskGenerationService> logger,
            IDatabaseService db,
            ITaskGenerationService taskGenerationService)
        {
            _logger = logger;
            _db = db;
            _taskGenerationService = taskGenerationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AGV任务生成服务已启动");
            int consecutiveErrorCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    //ImportLocationsFromExcel();


                    await ProcessTasks();
                    // _logger.LogInformation("完成执行ProcessTasks");

                    // 成功执行后重置错误计数
                    consecutiveErrorCount = 0;
                    
                    // 正常间隔
                    await Task.Delay(1000, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // 应用正在关闭，忽略此异常
                    _logger.LogInformation("AGV任务生成服务正在关闭");
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrorCount++;
                    
                    _logger.LogError(ex, $"生成AGV任务时发生错误 (第{consecutiveErrorCount}次连续错误)");
                    
                    // 根据连续错误次数增加等待时间
                    int delayMs = Math.Min(consecutiveErrorCount * 1000, 10000); // 最多等待10秒
                    _logger.LogWarning($"等待{delayMs/1000}秒后重试...");
                    
                    try
                    {
                        await Task.Delay(delayMs, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        // 应用正在关闭，忽略此异常
                        _logger.LogInformation("AGV任务生成服务正在关闭");
                        break;
                    }
                    catch (Exception delayEx)
                    {
                        // 即使延迟出错也不应该导致服务停止
                        _logger.LogError(delayEx, "延迟等待过程中发生错误");
                    }
                }
            }
        }

        private async Task ProcessTasks()
        {
            // 使用超时控制
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // 10秒超时
            
            try
            {
                using var conn = _db.CreateConnection();
                
                // 添加命令超时
                var command = new CommandDefinition(
                    "SELECT * FROM RCS_IODevices WHERE IsEnabled = 1",
                    commandTimeout: 5); // 5秒超时
                    
                var devices = await conn.QueryAsync<RCS_IODevices>(command);

                foreach (var device in devices)
                {
                    try
                    {
                        // 创建带超时的CommandDefinition
                        command = new CommandDefinition(
                            "SELECT * FROM RCS_IOSignals WHERE DeviceId = @DeviceId",
                            new { DeviceId = device.Id },
                            commandTimeout: 5); // 5秒超时
                            
                        var signals = (await conn.QueryAsync<RCS_IOSignals>(command)).ToList();

                        if (device.Id <= 27) // 上料架任务
                        {
                            // 1. 首先检查 DI7 信号
                            var di7Signal = signals.FirstOrDefault(s => s.Address == "DI7");
                            if (di7Signal == null || di7Signal.Value != 1)
                            {
                                // DI7 信号不存在或未激活，清除该设备的计时
                                //_di7SignalStartTimes.TryRemove(device.Id.ToString(), out _);
                                continue;
                            }

                            // 2. 检查 DI7 信号持续时间
                            //var now = DateTime.Now;
                            //var signalStartTime = _di7SignalStartTimes.GetOrAdd(device.Id.ToString(), now);
                            
                            //if ((now - signalStartTime).TotalSeconds >= 1) // DI7 信号已持续 1 秒
                            //{
                                // 3. 检查 DI1-DI6 的信号
                                var diSignals = signals.Where(s => 
                                    s.Address.StartsWith("DI") && 
                                    s.Address != "DI7" &&
                                    int.TryParse(s.Address.Substring(2), out int num) && 
                                    num <= 6 &&
                                    s.Value == 0).ToList();

                                // 4. 为每个激活的信号生成任务
                                foreach (var signal in diSignals)
                                {
                                    try
                                    {
                                        await _taskGenerationService.GenerateAGVTask(signal, device);
                                    }
                                    catch (Exception signalEx)
                                    {
                                        // 单个信号处理失败不应该影响其他信号
                                        _logger.LogError(signalEx, $"处理设备 {device.Id} 的信号 {signal.Address} 时发生错误");
                                    }
                                }
                            //}
                        }
                        else if (device.Id >= 29) // 下料任务
                        {
                            foreach (var signal in signals)
                            {
                                try
                                {
                                    if (signal.Value == 1 && signal.Address=="DI1")
                                    {
                                        _logger.LogInformation($"捕捉到{device.Id}的DI1信号闭合-1");

                                      
                                        await _taskGenerationService.GenerateAGVTask(signal, device);
                                         
                                    }
                                    else
                                    {
                                        // 如果信号变为OFF，清除对应的计时
                                        //_downlineSignalStartTimes.TryRemove($"{device.Id}_{signal.Address}", out _);
                                    }
                                }
                                catch (Exception signalEx)
                                {
                                    // 单个信号处理失败不应该影响其他信号
                                    _logger.LogError(signalEx, $"处理设备 {device.Id} 的信号 {signal.Address} 时发生错误");
                                }
                            }
                        }
                    }
                    catch (Exception deviceEx)
                    {
                        // 单个设备处理失败不应该影响其他设备
                        _logger.LogError(deviceEx, $"处理设备 {device.Id} 时发生错误");
                    }
                }

                // 清理过期的信号记录
                try
                {
                    CleanupExpiredSignals();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理过期信号记录时发生错误");
                }
            }
            catch (Exception ex) when (ex is TimeoutException || ex is TaskCanceledException)
            {
                _logger.LogWarning("任务处理操作超时");
                throw new TimeoutException("处理AGV任务生成超时", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务生成服务执行出错");
                throw; // 重新抛出异常，让上层处理重试逻辑
            }
        }

        private void CleanupExpiredSignals()
        {
            try
            {
                var now = DateTime.Now;
                var expiredTime = TimeSpan.FromMinutes(10); // 10分钟后清理

                // 清理上料架信号记录
                var expiredDi7Signals = _di7SignalStartTimes
                    .Where(kvp => (now - kvp.Value) > expiredTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredDi7Signals)
                {
                    _di7SignalStartTimes.TryRemove(key, out _);
                }

                // 清理下料信号记录
                var expiredDownlineSignals = _downlineSignalStartTimes
                    .Where(kvp => (now - kvp.Value) > expiredTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredDownlineSignals)
                {
                    _downlineSignalStartTimes.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                // 清理过期信号失败不应该影响主要功能
                _logger.LogError(ex, "清理过期信号记录时发生错误");
            }
        }

        /// <summary>
        /// 仅一次性导入库位Excel数据，适配截图表头，后续可屏蔽此方法
        /// </summary>
        private async Task ImportLocationsFromExcel()
        {
            string filePath = @"D:\import\locations.xlsx";
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning($"Excel文件未找到: {filePath}");
                return;
            }

            var locationsToInsert = new List<RCS_Locations>();
            int skipped = 0;
            int totalRows = 0;
            try
            {
                using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook(fs);
                    var sheet = workbook.GetSheetAt(0);
                    int rowCount = sheet.LastRowNum;

                    string lastGroup = null;
                    string lastNodeRemark = null;
                    string lastStation = null;
                    // 假设表头在第1行（索引0），数据从第2行（索引1）开始
                    for (int i = 1; i <= rowCount; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null) continue;
                        totalRows++;

                        // 分区
                        string group = (row.GetCell(0)?.ToString()?.Trim() ?? "") + (row.GetCell(1)?.ToString()?.Trim() ?? "");
                        if (string.IsNullOrEmpty(group)) group = lastGroup; else lastGroup = group;

                        // 库位名称
                        string nodeRemark = row.GetCell(2)?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(nodeRemark)) nodeRemark = lastNodeRemark; else lastNodeRemark = nodeRemark;

                        // 站台号
                        string station = row.GetCell(3)?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(station)) station = lastStation; else lastStation = station;

                        // 层数
                        string layerStr = row.GetCell(5)?.ToString()?.Trim();
                        int layer = 1;
                        int.TryParse(layerStr, out layer);
                        // 首层高度
                        string firstHeightStr = row.GetCell(6)?.ToString()?.Trim();
                        int firstHeight = 30;
                        int.TryParse(firstHeightStr, out firstHeight);
                        // 二层高度
                        string secondHeightStr = row.GetCell(7)?.ToString()?.Trim();
                        int secondHeight = 640;
                        int.TryParse(secondHeightStr, out secondHeight);

                        if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(nodeRemark) || string.IsNullOrEmpty(station))
                        {
                            skipped++;
                            _logger.LogWarning($"跳过第{i + 1}行，数据不全: group={group}, nodeRemark={nodeRemark}, station={station}");
                            continue;
                        }

                        if (layer == 2)
                        {
                            // 首层
                            locationsToInsert.Add(new RCS_Locations
                            {
                                Name = station,
                                NodeRemark = nodeRemark + "-1",
                                Group = group,
                                WattingNode = station,
                                MaterialCode = null,
                                PalletID = "0",
                                Weight = "0",
                                Quanitity = "空",
                                EntryDate = null,
                                LiftingHeight = firstHeight,
                                Lock = false
                            });
                            // 二层
                            locationsToInsert.Add(new RCS_Locations
                            {
                                Name = station,
                                NodeRemark = nodeRemark + "-2",
                                Group = group,
                                WattingNode = station,
                                MaterialCode = null,
                                PalletID = "0",
                                Weight = "0",
                                Quanitity = "空",
                                EntryDate = null,
                                LiftingHeight = secondHeight,
                                Lock = false
                            });
                        }
                        else
                        {
                            locationsToInsert.Add(new RCS_Locations
                            {
                                Name = station,
                                NodeRemark = nodeRemark,
                                Group = group,
                                WattingNode = station,
                                MaterialCode = null,
                                PalletID = "0",
                                Weight = "0",
                                Quanitity = "空",
                                EntryDate = null,
                                LiftingHeight = firstHeight,
                                Lock = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取Excel文件失败");
                return;
            }

            int inserted = 0;
            // 插入数据库，避免重复
            using var conn = _db.CreateConnection();
            foreach (var loc in locationsToInsert)
            {
                // 判断是否已存在（NodeRemark和Group联合唯一）
                var exists = await conn.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(1) FROM RCS_Locations WHERE NodeRemark = @NodeRemark AND [Group] = @Group",
                    new { loc.NodeRemark, loc.Group });
                if (exists > 0)
                {
                    _logger.LogInformation($"已存在: {loc.NodeRemark} - {loc.Group}, 跳过");
                    continue;
                }
                // 插入
                await conn.ExecuteAsync(@"INSERT INTO RCS_Locations
                    (Name, NodeRemark, MaterialCode, PalletID, Weight, Quanitity, EntryDate, [Group], LiftingHeight, Lock, WattingNode)
                    VALUES (@Name, @NodeRemark, @MaterialCode, @PalletID, @Weight, @Quanitity, @EntryDate, @Group, @LiftingHeight, @Lock, @WattingNode)", loc);
                inserted++;
            }
            _logger.LogInformation($"Excel导入完成，总行数: {totalRows}, 跳过: {skipped}, 实际插入: {inserted}, 待插入: {locationsToInsert.Count}");
        }
    }
} 