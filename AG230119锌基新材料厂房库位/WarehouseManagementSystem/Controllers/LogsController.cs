using Microsoft.AspNetCore.Mvc;

namespace WarehouseManagementSystem.Controllers
{
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private readonly string _logPath;

        public LogsController(ILogger<LogsController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _logPath = Path.Combine(env.ContentRootPath, "Logs");
        }

        public IActionResult Index()
        {
            try
            {
                // 获取今天的日志文件名（匹配新的命名格式）
                string today = DateTime.Now.ToString("yyyyMMdd");
                string currentLogFile = $"RCS-Pad-{today}.log";
                
                // 如果当前日志文件不存在，尝试查找带有数字后缀的文件
                if (!System.IO.File.Exists(Path.Combine(_logPath, currentLogFile)))
                {
                    var logFiles = Directory.GetFiles(_logPath, $"RCS-Pad-{today}*.log")
                                          .OrderByDescending(f => f)
                                          .ToList();
                    
                    if (logFiles.Any())
                    {
                        currentLogFile = Path.GetFileName(logFiles.First());
                    }
                }

                var logEntries = GetCurrentLogs(currentLogFile);
                return View(logEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取日志文件时发生错误");
                return View(new List<LogEntry>());
            }
        }

        [HttpGet]
        public IActionResult GetLatestLogs()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                string currentLogFile = $"RCS-Pad-{today}.log";
                
                // 如果当前日志文件不存在，尝试查找带有数字后缀的文件
                if (!System.IO.File.Exists(Path.Combine(_logPath, currentLogFile)))
                {
                    var logFiles = Directory.GetFiles(_logPath, $"RCS-Pad-{today}*.log")
                                          .OrderByDescending(f => f)
                                          .ToList();
                    
                    if (logFiles.Any())
                    {
                        currentLogFile = Path.GetFileName(logFiles.First());
                    }
                }

                var logEntries = GetCurrentLogs(currentLogFile);
                return Json(new { success = true, data = logEntries });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新日志时发生错误");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<LogEntry> GetCurrentLogs(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_logPath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return new List<LogEntry>();
                }

                return ReadLastLines(filePath, 1000)
                    .Select(line => ParseLogLine(line))
                    .Where(log => log != null)
                    .OrderByDescending(log => log.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取日志 {fileName} 失败");
                return new List<LogEntry>();
            }
        }

        private IEnumerable<string> ReadLastLines(string filePath, int lineCount)
        {
            try
            {
                var lines = new List<string>();
                var buffer = new char[1024];
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            lines.Add(line);
                            if (lines.Count > lineCount)
                            {
                                lines.RemoveAt(0);
                            }
                        }
                    }
                }
                return lines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取文件 {filePath} 失败");
                return Enumerable.Empty<string>();
            }
        }

        private LogEntry ParseLogLine(string line)
        {
            try
            {
                // 解析日志行，假设格式为："{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message}"
                var match = System.Text.RegularExpressions.Regex.Match(line,
                    @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}.\d{3} [\+\-]\d{2}:\d{2}) \[(?<level>\w+)\] (?<message>.*)$");

                if (match.Success)
                {
                    return new LogEntry
                    {
                        Timestamp = DateTime.Parse(match.Groups["timestamp"].Value),
                        Level = match.Groups["level"].Value,
                        Message = match.Groups["message"].Value
                    };
                }
            }
            catch (Exception ex)
            {
                // 在这里不记录日志，避免循环记录
                // 如果解析失败，返回null
            }
            return null;
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
    }
}
