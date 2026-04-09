using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using System.Collections.Concurrent;
using System.IO;

using WarehouseManagementSystem.Hubs;
using WarehouseManagementSystem.Hubs.TcpClient.Hubs;
using WarehouseManagementSystem.Models.TcpService;
using WarehouseManagementSystem.Service.TcpService;
using WarehouseManagementSystem.Service.Io;
using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Service.WepApi;
using WarehouseManagementSystem.Service.Plc;
using Microsoft.Data.SqlClient;
using System.Data;
using Services.Tasks;
using WarehouseManagementSystem.Service.Tasks;
using WarehouseManagementSystem.Services;

try
{
var builder = WebApplication.CreateBuilder(args);

// 确保日志目录存在
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// 配置 Serilog
var logPath = Path.Combine(logDirectory, "RCS-Pad-.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Filter.ByExcluding(logEvent =>
        logEvent.MessageTemplate.Text.Contains("Request starting HTTP") ||
        logEvent.MessageTemplate.Text.Contains("Executing endpoint") ||
        logEvent.MessageTemplate.Text.Contains("Executed endpoint") ||
        logEvent.MessageTemplate.Text.Contains("Request finished") ||
        logEvent.MessageTemplate.Text.Contains("Route matched") ||
        logEvent.MessageTemplate.Text.Contains("Executing action") ||
        logEvent.MessageTemplate.Text.Contains("Executed action"))
    .Enrich.FromLogContext()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
    .WriteTo.File(logPath,
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Debug,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 31,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1)
    )
    .CreateLogger();

// 使用 Serilog
builder.Host.UseSerilog();

// 添加数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// 添加数据库连接注册
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// 注册 LocationService
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddControllersWithViews();


// 添加控制器和视图支持
#region 新服务添加
builder.Services.AddSingleton<ConnectionStatusService>();
builder.Services.AddSingleton<IClientManagerService, ClientManagerService>(); // 注册 IClientManagerService
builder.Services.AddSingleton<IClientDataService, ClientDataService>(); // 如果未注册 IClientDataService，也需要注册
builder.Services.AddSingleton<IMessageHistoryService, MessageHistoryService>();
// 注册数据清理服务
builder.Services.AddSignalR();
builder.Services.AddHostedService<DataCleanupService>();
//builder.Services.AddHostedService<TcpServerService>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

//IO
//方法注册
builder.Services.AddSingleton<IIOService, IOService>();
builder.Services.AddSingleton<IIODeviceService, IODeviceService>();
builder.Services.AddSingleton<ITaskGenerationService, TaskGenerationService>();

    // 添加系统到期检查服务
    builder.Services.AddSingleton<ISystemExpirationService, SystemExpirationService>();

//读取信号
builder.Services.AddHostedService<StartupService>();
builder.Services.AddHostedService<WarehouseManagementSystem.Service.Tasks.AGVTaskGenerationService>();
//builder.Services.AddHostedService<WarehouseManagementSystem.Service.DiagnosticService>();


#endregion


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5055); // 监听所有网络接口
});

var app = builder.Build();

// 配置中间件
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

    // 添加全局异常处理中间件
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "未处理的异常");
            // 如果响应已经开始，则无法修改状态码
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                    <title>系统错误</title>
                    <style>
                        body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; background-color: #f8f9fa; text-align: center; padding: 50px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                        h1 {{ color: #dc3545; }}
                        p {{ font-size: 18px; color: #343a40; line-height: 1.6; }}
                        .footer {{ margin-top: 30px; font-size: 14px; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>系统发生错误</h1>
                        <p>系统遇到了一个问题，请联系系统管理员。</p>
                        <p>错误已被记录，我们会尽快解决。</p>
                        <div class='footer'>
                            &copy; {DateTime.Now.Year} 仓库管理系统 - 所有权利保留
                        </div>
                    </div>
                </body>
                </html>");
            }
        }
    });

    // 添加系统访问时间限制中间件
    app.Use(async (context, next) =>
    {
        // 检查是否是静态文件请求，静态文件请求不做限制
        if (!context.Request.Path.StartsWithSegments("/css") && 
            !context.Request.Path.StartsWithSegments("/js") &&
            !context.Request.Path.StartsWithSegments("/lib") &&
            !context.Request.Path.StartsWithSegments("/favicon.ico"))
        {
            var expirationDateString = app.Configuration["SystemAccess:ExpirationDate"];
            if (!string.IsNullOrEmpty(expirationDateString) && 
                DateTime.TryParse(expirationDateString, out var expirationDate))
            {
                if (DateTime.Now > expirationDate)
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync($@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>系统已过期</title>
                        <style>
                            body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; background-color: #f8f9fa; text-align: center; padding: 50px; }}
                            .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                            h1 {{ color: #dc3545; }}
                            p {{ font-size: 18px; color: #343a40; line-height: 1.6; }}
                            .footer {{ margin-top: 30px; font-size: 14px; color: #6c757d; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>系统使用期限已过</h1>
                            <p>您的系统使用许可已于 {expirationDate:yyyy年MM月dd日} 到期。</p>
                            <p>请联系系统管理员续期或获取新的访问权限。</p>
                            <div class='footer'>
                                &copy; {DateTime.Now.Year} 仓库管理系统 - 所有权利保留
                            </div>
                        </div>
                    </body>
                    </html>");
                    return;
                }
            }
        }
        
        await next();
    });

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<TcpHub>("/tcpHub");

app.MapHub<TcpClientHub>("/tcpClientHub");

app.MapHub<SignalHub>("/signalHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DisplayLocation}/{action=Index}/{id?}");

    try
    {
app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "应用程序意外终止");
    }
    finally
    {
        Log.CloseAndFlush();
    }
}
catch (Exception ex)
{
    // 确保即使在启动过程中发生异常也能记录日志
    if (Log.Logger != null && Log.Logger is not Serilog.Core.Logger)
    {
        Log.Fatal(ex, "应用程序启动失败");
        Log.CloseAndFlush();
    }
    else
    {
        // 如果日志系统尚未初始化，则输出到控制台
        Console.WriteLine($"应用程序启动失败: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        
        // 防止控制台立即关闭
        Console.WriteLine("\n按任意键继续...");
        Console.ReadKey();
    }
}

// 类型声明放在顶级语句之后
public class MessageCache
{
    private static readonly ConcurrentDictionary<string, DateTime> _cache = new();
    private const int DuplicateTimeWindowMinutes = 3;

    public static bool ShouldLog(string message)
    {
        var now = DateTime.Now;
        if (_cache.TryGetValue(message, out var lastTime))
        {
            if ((now - lastTime).TotalMinutes < DuplicateTimeWindowMinutes)
            {
                return false;
            }
        }
        _cache.AddOrUpdate(message, now, (_, _) => now);
        return true;
    }

    public static void Cleanup()
    {
        var now = DateTime.Now;
        var expiredMessages = _cache.Where(x => (now - x.Value).TotalMinutes >= DuplicateTimeWindowMinutes)
                                  .Select(x => x.Key)
                                  .ToList();
        foreach (var message in expiredMessages)
        {
            _cache.TryRemove(message, out _);
        }
    }
}

public class CacheCleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
        while (!stoppingToken.IsCancellationRequested)
            {
                try
        {
            MessageCache.Cleanup();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "缓存清理过程中发生错误");
                }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // 正常取消，不需要处理
        }
        catch (Exception ex)
        {
            Log.Error(ex, "缓存清理服务异常终止");
        }
    }
}
