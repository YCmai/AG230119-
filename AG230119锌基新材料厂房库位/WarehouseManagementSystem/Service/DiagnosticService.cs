using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Service
{
    public class DiagnosticService : BackgroundService
    {
        private readonly ILogger<DiagnosticService> _logger;

        public DiagnosticService(ILogger<DiagnosticService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("诊断服务已启动");
            int consecutiveErrorCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 获取当前进程
                    Process process = null;
                    try
                    {
                        process = Process.GetCurrentProcess();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取当前进程失败");
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                        continue;
                    }
                    
                    // 获取内存使用情况
                    long memoryInMB = 0;
                    try
                    {
                        memoryInMB = process.WorkingSet64 / (1024 * 1024);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取内存使用情况失败");
                    }
                    
                    // 获取线程数量
                    int threadCount = 0;
                    try
                    {
                        threadCount = process.Threads.Count;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取线程数量失败");
                    }
                    
                    // 获取处理器时间
                    TimeSpan cpuTime = TimeSpan.Zero;
                    try
                    {
                        cpuTime = process.TotalProcessorTime;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取处理器时间失败");
                    }
                    
                    // 获取句柄数
                    int handleCount = 0;
                    try
                    {
                        handleCount = process.HandleCount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取句柄数失败");
                    }
                    
                    // 获取GC信息
                    long gcMemory = 0;
                    int gen0Count = 0, gen1Count = 0, gen2Count = 0;
                    try
                    {
                        gcMemory = GC.GetTotalMemory(false) / (1024 * 1024);
                        gen0Count = GC.CollectionCount(0);
                        gen1Count = GC.CollectionCount(1);
                        gen2Count = GC.CollectionCount(2);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取GC信息失败");
                    }

                    _logger.LogInformation(
                        "系统诊断: 内存={MemoryMB}MB, GC内存={GcMemoryMB}MB, 线程数={ThreadCount}, 句柄数={HandleCount}, " +
                        "CPU时间={CpuTime}, GC次数=[G0:{Gen0}, G1:{Gen1}, G2:{Gen2}]",
                        memoryInMB, gcMemory, threadCount, handleCount, cpuTime, gen0Count, gen1Count, gen2Count);

                    // 检查异常情况
                    if (memoryInMB > 1000) // 超过1GB内存
                    {
                        _logger.LogWarning("内存使用过高: {MemoryMB}MB", memoryInMB);
                    }
                    
                    if (threadCount > 100) // 线程数过多
                    {
                        _logger.LogWarning("线程数过多: {ThreadCount}", threadCount);
                    }
                    
                    if (handleCount > 1000) // 句柄数过多
                    {
                        _logger.LogWarning("句柄数过多: {HandleCount}", handleCount);
                    }
                    
                    // 成功执行后重置错误计数
                    consecutiveErrorCount = 0;
                }
                catch (TaskCanceledException)
                {
                    // 应用正在关闭，忽略此异常
                    _logger.LogInformation("诊断服务正在关闭");
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrorCount++;
                    _logger.LogError(ex, $"诊断服务执行出错 (第{consecutiveErrorCount}次连续错误)");
                    
                    // 如果连续错误次数过多，增加等待时间以减少资源消耗
                    if (consecutiveErrorCount > 3)
                    {
                        _logger.LogWarning($"诊断服务连续出错{consecutiveErrorCount}次，等待时间增加");
                    }
                }

                try
                {
                    // 每分钟执行一次，或者根据错误次数增加等待时间
                    var delayTime = consecutiveErrorCount > 3 
                        ? TimeSpan.FromMinutes(Math.Min(consecutiveErrorCount, 10)) // 最多等待10分钟
                        : TimeSpan.FromMinutes(1);
                    
                    await Task.Delay(delayTime, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // 应用正在关闭，忽略此异常
                    _logger.LogInformation("诊断服务正在关闭");
                    break;
                }
                catch (Exception ex)
                {
                    // 延迟出错不应该导致服务停止
                    _logger.LogError(ex, "诊断服务延迟等待时发生错误");
                }
            }
            
            _logger.LogInformation("诊断服务已停止");
        }
    }
} 