using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

using WarehouseManagementSystem.Models.WebApi;

namespace WarehouseManagementSystem.Service.WepApi
{
    public class ApiTaskExecuteService : BackgroundService
    {
        private readonly ILogger<ApiTaskExecuteService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ApiTaskExecuteService(
            ILogger<ApiTaskExecuteService> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var taskService = scope.ServiceProvider.GetRequiredService<IWebApiTaskService>();
                        var tasks = await taskService.GetUnexecutedTasksAsync();

                        foreach (var task in tasks)
                        {
                            var apiUrl = _configuration["TaskSettings:ApiUrl"];
                            using (var client = new HttpClient())
                            {
                                try
                                {
                                    // 构建请求参数
                                    var requestData = new
                                    {
                                        requestCode = task.RequestCode,
                                        taskCode = task.TaskCode,
                                        taskStatus = task.TaskStatus,
                                        // 可以根据需要添加其他参数
                                    };

                                    // 设置请求头
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    // 序列化请求数据
                                    var jsonContent = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
                                    {
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    });
                                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                                    // 发送请求
                                    var response = await client.PostAsync(apiUrl, content);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        var responseContent = await response.Content.ReadAsStringAsync();
                                        _logger.LogInformation($"接口任务执行成功. Response: {responseContent}");

                                        task.Execute = true;
                                        task.ExecuteResult = $"Success: {responseContent}";
                                    }
                                    else
                                    {
                                        var errorContent = await response.Content.ReadAsStringAsync();
                                        _logger.LogWarning($"接任任务执行失败. Status: {response.StatusCode}, Error: {errorContent}");

                                        task.RetryCount++;
                                        task.ExecuteResult = $"Failed: {response.StatusCode} - {errorContent}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"接口执行失败 {task.TaskCode}");
                                    task.RetryCount++;
                                    task.ExecuteResult = $"Error: {ex.Message}";
                                }

                                task.UpdateTime = DateTime.Now;
                                await taskService.UpdateTaskAsync(task);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "接口执行失败");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
