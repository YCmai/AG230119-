using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace WMS
{
    public class Program
    {
       


        public static int Main(string[] args)
        {
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logDirectory);

            string infoPath = Path.Combine(logDirectory, "Info", ".log");
            string warningPath = Path.Combine(logDirectory, "Warning", ".log");
            string errorPath = Path.Combine(logDirectory, "Error", ".log");
            string fatalPath = Path.Combine(logDirectory, "Fatal", ".log");

            string template = "{NewLine}时间:{Timestamp:yyyy-MM-dd HH:mm:ss}{NewLine}" +
                              "等级:{Level}{NewLine}" +
                              "来源:{SourceContext}{NewLine}" +
                              "具体消息:{Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(lev => lev.Level == LogEventLevel.Information)
                    .WriteTo.Async(config => config.File(
                        infoPath,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 1024 * 1024 * 1,
                        retainedFileCountLimit: 10,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        outputTemplate: template)))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(lev => lev.Level == LogEventLevel.Warning)
                    .WriteTo.Console(new JsonFormatter())
                    .WriteTo.Async(config => config.File(
                        warningPath,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 1024 * 1024 * 1,
                        retainedFileCountLimit: 10,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        outputTemplate: template)))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(lev => lev.Level == LogEventLevel.Error)
                    .WriteTo.Console(new JsonFormatter())
                    .WriteTo.Async(config => config.File(
                        errorPath,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 1024 * 1024 * 1,
                        retainedFileCountLimit: 10,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        outputTemplate: template)))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(lev => lev.Level == LogEventLevel.Fatal)
                    .WriteTo.Console(new JsonFormatter())
                    .WriteTo.Async(config => config.File(
                        fatalPath,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 1024 * 1024 * 1,
                        retainedFileCountLimit: 10,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        outputTemplate: template)))
                // Filter out Information level logs from the console
                .WriteTo.Logger(lg => lg.Filter.ByExcluding(lev => lev.Level == LogEventLevel.Information)
                    .WriteTo.Console(outputTemplate: template, theme: CustomConsoleTheme))
                .CreateLogger();

            try
            {
                Log.Information("Starting WMS.HttpApi.Host.");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        private static readonly AnsiConsoleTheme CustomConsoleTheme = new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[37m", // 白色
            [ConsoleThemeStyle.SecondaryText] = "\x1b[90m", // 灰色
            [ConsoleThemeStyle.TertiaryText] = "\x1b[90m", // 灰色
            [ConsoleThemeStyle.Invalid] = "\x1b[33m", // 黄色
            [ConsoleThemeStyle.Null] = "\x1b[94m", // 蓝色
            [ConsoleThemeStyle.Name] = "\x1b[37m", // 白色
            [ConsoleThemeStyle.String] = "\x1b[36m", // 青色
            [ConsoleThemeStyle.Number] = "\x1b[95m", // 洋红色
            [ConsoleThemeStyle.Boolean] = "\x1b[94m", // 蓝色
            [ConsoleThemeStyle.Scalar] = "\x1b[92m", // 绿色
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[90m", // 灰色
            [ConsoleThemeStyle.LevelDebug] = "\x1b[90m", // 灰色
            [ConsoleThemeStyle.LevelInformation] = "\x1b[37m", // 白色
            [ConsoleThemeStyle.LevelWarning] = "\x1b[33m", // 黄色
            [ConsoleThemeStyle.LevelError] = "\x1b[31m", // 红色
            [ConsoleThemeStyle.LevelFatal] = "\x1b[32m" // 绿色
        });



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(build =>
                {
                    build.AddJsonFile("appsettings.secrets.json", optional: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://0.0.0.0:5000")  //添加对所有ip都可以访问
                              .UseStartup<Startup>();
                })
                .UseAutofac()
                .UseSerilog();
    }
}
