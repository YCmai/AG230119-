using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Dapper;
using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Hubs;
using WarehouseManagementSystem.Models.TcpService;
using WarehouseManagementSystem.Service.Io;
using System.Text.Json;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Service.TcpService
{
    // Services/MessageHistoryService.cs
    public class MessageHistoryService : IMessageHistoryService, IMessageCleanupService
    {
        private readonly ConcurrentQueue<RCS_TcpClientMessages> _messageHistory = new();
        private readonly ConcurrentDictionary<string, RCS_TcpClientStatuses> _clientStatuses = new();
        private readonly IHubContext<TcpHub> _hubContext;
        private readonly IDatabaseService _db;
        private readonly ILogger<MessageHistoryService> _logger;
        public MessageHistoryService(IHubContext<TcpHub> hubContext, ILogger<MessageHistoryService> logger, IDatabaseService db)
        {
            _hubContext = hubContext;
            _db= db;
            _logger = logger;
        }

        public enum StationType
        {
            PlatingLine, // 电镀线 A1
            ProcessingLine, // 301-306线 B1-B12
            NGStation // NG工位 C1
        }

        public enum CommandType
        {
            CallAGV, // X - 呼叫AGV
            AGVResponse, // Y - AGV应答
            AGVArriveOutside, // M - AGV到达外围
            PLCAllowEnter, // N - PLC允许进入
            AGVArrivePosition, // E - AGV到达工位
            PLCAllowOperation, // F - PLC允许操作
            AGVOperationComplete, // K - AGV操作完成
            PLCAllowLeave // T - PLC允许离开
        }

        public class ProtocolData
        {
            public StationType StationType { get; set; }
            public string StationNumber { get; set; }
            public CommandType CommandType { get; set; }
            public int? Position { get; set; }
            public string? Code { get; set; } // 二维码
            public bool IsNG { get; set; }
        }

        public string GetNextCommand(string currentCommand)
        {
            return currentCommand switch
            {
                "X" => "Y",  // 收到呼叫后，返回应答
                "Y" => "M",  // 应答后，AGV到达外围
                "M" => "N",  // AGV到达外围后，等待PLC允许进入
                "N" => "E",  // PLC允许进入后，AGV到达工位
                "E" => "F",  // AGV到达工位后，等待PLC允许操作
                "F" => "K",  // PLC允许操作后，AGV完成操作
                "K" => "T",  // AGV完成操作后，等待PLC允许离开
                "T" => null, // 流程结束
                _ => null
            };
        }

        // 添加一个方法来检查是否需要自动响应
        public bool NeedsAutoResponse(string command)
        {
            // AGV需要自动响应的命令
            return command switch
            {
                "X" => true,  // 需要响应呼叫
                "N" => true,  // 需要确认到达工位
                "F" => true,  // 需要确认操作完成
                _ => false
            };
        }



        public async Task AddMessageAsync(string clientEndpoint, string message, MessageType type = MessageType.Data)
        {
            try
            {
                // 解析协议数据
                var protocolData = ParseProtocol(message);
                var parsedDataJson = JsonSerializer.Serialize(protocolData);

                using var conn = _db.CreateConnection();

                // 插入消息记录
                const string insertMessageSql = @"
                INSERT INTO RCS_TcpClientMessages (
                    ClientEndpoint, Message, Timestamp, MessageType, 
                    ProcessStatus, CreateTime, ParsedData
                ) VALUES (
                    @ClientEndpoint, @Message, @Timestamp, @MessageType, 
                    @ProcessStatus, @CreateTime, @ParsedData
                );
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var messageId = await conn.ExecuteScalarAsync<int>(insertMessageSql, new
                {
                    ClientEndpoint = clientEndpoint,
                    Message = message,
                    Timestamp = DateTime.Now,
                    MessageType = type,
                    ProcessStatus = ProcessStatus.Pending,
                    CreateTime = DateTime.Now,
                    ParsedData = parsedDataJson
                });

                // 更新客户端状态 - 修正 MERGE 语句
                const string upsertStatusSql = @"
            MERGE RCS_TcpClientStatuses AS target
            USING (SELECT @ClientEndpoint as ClientEndpoint) AS source
            ON target.ClientEndpoint = source.ClientEndpoint
            WHEN MATCHED THEN
                UPDATE SET 
                    LastActiveTime = @LastActiveTime,
                    MessageCount = MessageCount + 1,
                    UpdateTime = @UpdateTime
            WHEN NOT MATCHED THEN
                INSERT (ClientEndpoint, IsConnected, LastActiveTime, MessageCount, CreateTime)
                VALUES (@ClientEndpoint, @IsConnected, @LastActiveTime, 1, @CreateTime)
            ;";  // 添加分号

                await conn.ExecuteAsync(upsertStatusSql, new
                {
                    ClientEndpoint = clientEndpoint,
                    IsConnected = true,
                    LastActiveTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });

                // 获取插入的消息记录
                var clientMessage = await conn.QueryFirstOrDefaultAsync<RCS_TcpClientMessages>(
                    "SELECT * FROM RCS_TcpClientMessages WHERE Id = @Id", new { Id = messageId });

                // 通过SignalR推送更新
                if (clientMessage != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", clientMessage);
                    var clientStatuses = await GetClientStatusesAsync();
                    await _hubContext.Clients.All.SendAsync("UpdateClientStatus", clientStatuses);
                }

                // 处理消息
                await ProcessMessageAsync(messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加消息记录时发生错误");
                throw;
            }
        }


        private ProtocolData ParseProtocol(string message)
        {
            try
            {
                var parts = message.Split(';');
                if (parts.Length < 2) throw new Exception("无效的协议格式");

                var stationNumber = parts[0];
                var command = parts[1];
                string code = parts.Length > 2 ? parts[2] : null;

                var protocolData = new ProtocolData
                {
                    StationNumber = stationNumber,
                    Code = code
                };

                // 解析站点类型
                if (stationNumber.StartsWith("A"))
                {
                    protocolData.StationType = StationType.PlatingLine;
                }
                else if (stationNumber.StartsWith("B"))
                {
                    protocolData.StationType = StationType.ProcessingLine;
                }
                else if (stationNumber.StartsWith("C"))
                {
                    protocolData.StationType = StationType.NGStation;
                }

                // 解析命令类型和位置
                if (command.StartsWith("NG"))
                {
                    protocolData.IsNG = true;
                    protocolData.Position = int.Parse(command.Substring(2));
                }
                else
                {
                    var cmdType = command[0];
                    protocolData.CommandType = cmdType switch
                    {
                        'X' => CommandType.CallAGV,
                        'Y' => CommandType.AGVResponse,
                        'M' => CommandType.AGVArriveOutside,
                        'N' => CommandType.PLCAllowEnter,
                        'E' => CommandType.AGVArrivePosition,
                        'F' => CommandType.PLCAllowOperation,
                        'K' => CommandType.AGVOperationComplete,
                        'T' => CommandType.PLCAllowLeave,
                        _ => throw new Exception($"未知的命令类型: {cmdType}")
                    };

                    if (command.Length > 1)
                    {
                        protocolData.Position = int.Parse(command.Substring(1));
                    }
                }

                return protocolData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析协议失败: {Message}", message);
                throw;
            }
        }



        // 获取分页消息列表
        public List<RCS_TcpClientMessages> GetMessages(int page, int pageSize, string clientEndpoint = null)
        {
            using var conn = _db.CreateConnection();
            var offset = (page - 1) * pageSize;

            var sql = @"
            SELECT *
            FROM RCS_TcpClientMessages
            WHERE (@ClientEndpoint IS NULL OR ClientEndpoint = @ClientEndpoint)
            ORDER BY Timestamp DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            return conn.Query<RCS_TcpClientMessages>(sql, new
            {
                ClientEndpoint = clientEndpoint,
                Offset = offset,
                PageSize = pageSize
            }).ToList();
        }

        /// <summary>
        /// 处理消息并执行相应的业务逻辑
        /// </summary>
        private async Task ProcessMessageAsync(int messageId)
        {
            try
            {
                using var conn = _db.CreateConnection();

                // 更新消息状态为处理中
                await conn.ExecuteAsync(@"
            UPDATE RCS_TcpClientMessages 
            SET ProcessStatus = @Status, 
                UpdateTime = @UpdateTime 
            WHERE Id = @Id",
                    new
                    {
                        Id = messageId,
                        Status = ProcessStatus.Processing,
                        UpdateTime = DateTime.UtcNow
                    });

                // 获取消息内容
                var message = await conn.QueryFirstOrDefaultAsync<RCS_TcpClientMessages>(@"
            SELECT * FROM RCS_TcpClientMessages WHERE Id = @Id",
                    new { Id = messageId });

                if (message != null)
                {
                    // 解析消息内容
                    var parsedData = await ParseMessageAsync(message);

                    // 根据解析结果执行相应的业务逻辑
                    if (parsedData != null)
                    {
                        await ExecuteBusinessLogicAsync(parsedData);
                    }

                    // 更新处理状态为完成
                    await conn.ExecuteAsync(@"
                UPDATE RCS_TcpClientMessages 
                SET ProcessStatus = @Status, 
                    ParsedData = @ParsedData,
                    UpdateTime = @UpdateTime
                WHERE Id = @Id",
                        new
                        {
                            Id = messageId,
                            Status = ProcessStatus.Completed,
                            ParsedData = JsonSerializer.Serialize(parsedData),
                            UpdateTime = DateTime.UtcNow
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理消息时发生错误: {MessageId}", messageId);

                // 更新处理状态为失败
                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(@"
            UPDATE RCS_TcpClientMessages 
            SET ProcessStatus = @Status, 
                UpdateTime = @UpdateTime
            WHERE Id = @Id",
                    new
                    {
                        Id = messageId,
                        Status = ProcessStatus.Failed,
                        UpdateTime = DateTime.UtcNow
                    });
            }
        }

        /// <summary>
        /// 解析消息内容
        /// </summary>
        private async Task<MessageData> ParseMessageAsync(RCS_TcpClientMessages message)
        {
            try
            {
                // 示例：解析不同类型的消息
                // 这里根据你的实际需求进行修改
                if (message.Message.StartsWith("{") && message.Message.EndsWith("}"))
                {
                    // JSON 格式消息
                    return JsonSerializer.Deserialize<MessageData>(message.Message);
                }
                else if (message.Message.StartsWith("<") && message.Message.EndsWith(">"))
                {
                    // XML 格式消息
                    // 添加 XML 解析逻辑
                }
                else
                {
                    // 自定义格式消息
                    // 例如：按特定分隔符分割
                    var parts = message.Message.Split('|');
                    if (parts.Length >= 3)
                    {
                        return new MessageData
                        {
                            Type = parts[0],
                            Command = parts[1],
                            Content = parts[2]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析消息失败: {Message}", message.Message);
            }

            return null;
        }

        /// <summary>
        /// 执行业务逻辑
        /// </summary>
        private async Task ExecuteBusinessLogicAsync(MessageData data)
        {
            using var conn = _db.CreateConnection();

            switch (data.Type.ToUpper())
            {
                case "COMMAND":
                    await ProcessCommandMessageAsync(data);
                    break;

                case "STATUS":
                    await ProcessStatusMessageAsync(data);
                    break;

                case "DATA":
                    await ProcessDataMessageAsync(data);
                    break;

                    // 添加更多消息类型处理...
            }
        }

        /// <summary>
        /// 消息数据模型
        /// </summary>
        public class MessageData
        {
            public string Type { get; set; }
            public string Command { get; set; }
            public string Content { get; set; }
            public Dictionary<string, object> Parameters { get; set; }
        }

        /// <summary>
        /// 处理命令消息
        /// </summary>
        private async Task ProcessCommandMessageAsync(MessageData data)
        {
            try
            {
                switch (data.Command.ToUpper())
                {
                    case "START":
                        // 处理启动命令
                        //await StartProcessAsync(data);
                        break;

                    case "STOP":
                        // 处理停止命令
                        //await StopProcessAsync(data);
                        break;

                    case "UPDATE":
                        // 处理更新命令
                        //await UpdateDataAsync(data);
                        break;

                        // 添加更多命令处理...
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理命令消息失败: {Command}", data.Command);
                throw;
            }
        }

        /// <summary>
        /// 处理状态消息
        /// </summary>
        private async Task ProcessStatusMessageAsync(MessageData data)
        {
            try
            {
                // 处理状态更新逻辑
                // 例如：更新设备状态、记录状态变化等
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理状态消息失败");
                throw;
            }
        }

        /// <summary>
        /// 处理数据消息
        /// </summary>
        private async Task ProcessDataMessageAsync(MessageData data)
        {
            try
            {
                // 处理数据记录逻辑
                // 例如：保存传感器数据、更新统计信息等
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理数据消息失败");
                throw;
            }
        }


        /// <summary>
        /// 获取客户端状态列表
        /// </summary>
        public async Task<List<RCS_TcpClientStatuses>> GetClientStatusesAsync()
        {
            using var conn = _db.CreateConnection();
            const string sql = "SELECT * FROM RCS_TcpClientStatuses ORDER BY LastActiveTime DESC";
            var result = await conn.QueryAsync<RCS_TcpClientStatuses>(sql);
            return result.ToList();
        }

        // 更新视图模型
        public MessageViewModel GetMessageViewModel(int page = 1, int pageSize = 10, string clientEndpoint = null)
        {
            return new MessageViewModel
            {
                Messages = GetMessages(page, pageSize, clientEndpoint),
                ClientStatuses = GetClientStatusesAsync().Result,
                CurrentPage = page,
                PageSize = pageSize,
                TotalMessages = GetTotalMessageCount(clientEndpoint),
                SelectedClient = clientEndpoint,
                Statistics = GetMessageStatistics(
                    DateTime.UtcNow.AddHours(-24),
                    DateTime.UtcNow
                )
            };
        }


        // 获取消息统计数据
        public Dictionary<DateTime, int> GetMessageStatistics(DateTime start, DateTime end)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
            SELECT 
                DATEADD(HOUR, DATEDIFF(HOUR, 0, Timestamp), 0) AS Hour,
                COUNT(*) AS MessageCount
            FROM RCS_TcpClientMessages
            WHERE Timestamp BETWEEN @Start AND @End
            GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, Timestamp), 0)
            ORDER BY Hour";

            var results = conn.Query<(DateTime Hour, int MessageCount)>(sql, new { Start = start, End = end })
                .ToDictionary(x => x.Hour, x => x.MessageCount);

            // 填充没有数据的小时
            var current = start;
            while (current <= end)
            {
                if (!results.ContainsKey(current))
                {
                    results[current] = 0;
                }
                current = current.AddHours(1);
            }

            return results.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        // 获取消息总数
        public int GetTotalMessageCount(string clientEndpoint = null)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
            SELECT COUNT(*)
            FROM RCS_TcpClientMessages
            WHERE (@ClientEndpoint IS NULL OR ClientEndpoint = @ClientEndpoint)";

            return conn.ExecuteScalar<int>(sql, new { ClientEndpoint = clientEndpoint });
        }

        public async Task UpdateClientStatusAsync(string clientEndpoint, bool isConnected)
        {
            try
            {
                using var conn = _db.CreateConnection();
                const string sql = @"
            MERGE RCS_TcpClientStatuses AS target
            USING (SELECT @ClientEndpoint as ClientEndpoint) AS source
            ON target.ClientEndpoint = source.ClientEndpoint
            WHEN MATCHED THEN
                UPDATE SET 
                    IsConnected = @IsConnected,
                    LastActiveTime = @LastActiveTime,
                    UpdateTime = @UpdateTime
            WHEN NOT MATCHED THEN
                INSERT (ClientEndpoint, IsConnected, LastActiveTime, MessageCount, CreateTime)
                VALUES (@ClientEndpoint, @IsConnected, @LastActiveTime, 0, @CreateTime)
            ;";  // 注意这里的分号

                await conn.ExecuteAsync(sql, new
                {
                    ClientEndpoint = clientEndpoint,
                    IsConnected = isConnected,
                    LastActiveTime = DateTime.UtcNow,
                    CreateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                });

                var clientStatuses = await GetClientStatusesAsync();
                await _hubContext.Clients.All.SendAsync("UpdateClientStatus", clientStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateClientStatusAsync处理消息时发生错误: {ex.Message}");
            }
        }

        // 实现清理接口
        public Task CleanupMessagesBeforeDateAsync(DateTime cutoffDate)
        {
            if (!_messageHistory.Any())
            {

                return Task.CompletedTask;
            }

            var originalCount = _messageHistory.Count;
            var messagesToKeep = _messageHistory
                .Where(m => m.Timestamp >= cutoffDate)
                .ToList();

            _messageHistory.Clear();
            foreach (var message in messagesToKeep)
            {
                _messageHistory.Enqueue(message);
            }

            var removedCount = originalCount - _messageHistory.Count;

            return Task.CompletedTask;
        }

        public Task<int> GetMessageCountBeforeDateAsync(DateTime cutoffDate)
        {
            var count = _messageHistory.Count(m => m.Timestamp < cutoffDate);
            return Task.FromResult(count);
        }

        /// <summary>
        /// 获取消息历史记录
        /// </summary>
        public async Task<List<RCS_TcpClientMessages>> GetMessageHistoryAsync(string clientEndpoint, int count = 100)
        {
            using var conn = _db.CreateConnection();
            const string sql = @"
            SELECT TOP (@Count) *
            FROM RCS_TcpClientMessages 
            WHERE ClientEndpoint = @ClientEndpoint
            ORDER BY Timestamp DESC";

            var result = await conn.QueryAsync<RCS_TcpClientMessages>(sql, new { ClientEndpoint = clientEndpoint, Count = count });
            return result.ToList();
        }

        public async Task<MessageStatistics> GetMessageStatisticsAsync()
        {
            using var conn = _db.CreateConnection();

            var sql = @"
        SELECT 
            ProcessStatus,
            COUNT(*) as Count
        FROM RCS_TcpClientMessages
        GROUP BY ProcessStatus";

            var results = await conn.QueryAsync<(ProcessStatus Status, int Count)>(sql);

            return new MessageStatistics
            {
                PendingCount = results.FirstOrDefault(x => x.Status == ProcessStatus.Pending).Count,
                ProcessingCount = results.FirstOrDefault(x => x.Status == ProcessStatus.Processing).Count,
                CompletedCount = results.FirstOrDefault(x => x.Status == ProcessStatus.Completed).Count,
                FailedCount = results.FirstOrDefault(x => x.Status == ProcessStatus.Failed).Count
            };
        }

        public async Task<List<RCS_TcpClientMessages>> GetRecentMessagesAsync(int count)
        {
            using var conn = _db.CreateConnection();

            var sql = @"
            SELECT TOP (@count) *
            FROM RCS_TcpClientMessages
            ORDER BY Timestamp DESC";

            var messages = await conn.QueryAsync<RCS_TcpClientMessages>(sql, new { count });
            return messages.ToList();
        }

        public async Task RetryMessageAsync(int messageId)
        {
            using var conn = _db.CreateConnection();

            // 重置消息状态
            await conn.ExecuteAsync(@"
            UPDATE RCS_TcpClientMessages
            SET ProcessStatus = @Status,
                UpdateTime = @UpdateTime
            WHERE Id = @Id",
                new
                {
                    Id = messageId,
                    Status = ProcessStatus.Pending,
                    UpdateTime = DateTime.UtcNow
                });

            // 重新处理消息
            await ProcessMessageAsync(messageId);
        }

        public async Task<List<RCS_TcpClientMessages>> GetPagedMessagesAsync(int page, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var offset = (page - 1) * pageSize;

            const string sql = @"
            SELECT *
            FROM RCS_TcpClientMessages
            ORDER BY Timestamp DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            var messages = await conn.QueryAsync<RCS_TcpClientMessages>(sql, new { Offset = offset, PageSize = pageSize });
            return messages.ToList();
        }

        public async Task<int> GetTotalMessageCountAsync()
        {
            using var conn = _db.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM RCS_TcpClientMessages";
            return await conn.ExecuteScalarAsync<int>(sql);
        }
    }


}
