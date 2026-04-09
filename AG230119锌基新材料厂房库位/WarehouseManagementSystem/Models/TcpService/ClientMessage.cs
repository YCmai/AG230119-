using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models.TcpService
{
    // Models/ClientMessage.cs
    /// <summary>
    /// TCP客户端消息记录表
    /// </summary>
    /// <summary>
    /// TCP客户端消息记录表
    /// </summary>
    public class RCS_TcpClientMessages
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 客户端终端地址（IP:端口）
        /// </summary>
        public string ClientEndpoint { get; set; }

        /// <summary>
        /// 原始消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 消息时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 消息类型（数据/心跳等）
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 消息处理状态（待处理/处理中/已完成/失败）
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 记录更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 关联的AGV任务ID（RCS_UserTasks表的外键）
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// 站点类型（电镀线/加工线/NG工位）
        /// </summary>
        public string StationType { get; set; }

        /// <summary>
        /// 站号（如A1/B1-B12/C1）
        /// </summary>
        public string StationNumber { get; set; }

        /// <summary>
        /// 工位号（如1,2,3,4）
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// 命令类型（X-呼叫/Y-应答/M-到达外围/N-允许进入/E-到达工位/F-允许操作/K-完成操作/T-允许离开）
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// 是否为NG响应（表示任务占用或异常）
        /// </summary>
        public bool IsNGResponse { get; set; }

        /// <summary>
        /// 二维码信息（如果有）
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 是否需要AGV程序自动响应
        /// </summary>
        public bool NeedResponse { get; set; }

        /// <summary>
        /// 响应状态（true成功/false失败）
        /// </summary>
        public bool? ResponseStatus { get; set; }

        /// <summary>
        /// 响应消息内容
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public DateTime? ResponseTime { get; set; }

        /// <summary>
        /// 交互步骤（1-8，对应八个命令类型的顺序）
        /// 1: 呼叫AGV(X)
        /// 2: AGV应答(Y)
        /// 3: AGV到达外围(M)
        /// 4: PLC允许进入(N)
        /// 5: AGV到达工位(E)
        /// 6: PLC允许操作(F)
        /// 7: AGV操作完成(K)
        /// 8: PLC允许离开(T)
        /// </summary>
        public int InteractionStep { get; set; }

        /// <summary>
        /// 当前交互是否完成
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 错误信息（如果有）
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// TCP客户端状态表
    /// </summary>
    public class RCS_TcpClientStatuses
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 客户端终端地址 (IP:Port)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ClientEndpoint { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// 消息计数
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 数据消息
        /// </summary>
        Data = 0,

        /// <summary>
        /// 状态消息
        /// </summary>
        Status = 1,

        /// <summary>
        /// 系统消息
        /// </summary>
        System = 2
    }

    /// <summary>
    /// 消息处理状态枚举
    /// </summary>
    public enum ProcessStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 3
    }

    // Services/IMessageHistoryService.cs
    public interface IMessageHistoryService
    {
        Task AddMessageAsync(string clientEndpoint, string message, MessageType type = MessageType.Data);
        List<RCS_TcpClientMessages> GetMessages(int page, int pageSize, string clientEndpoint = null);
        Dictionary<DateTime, int> GetMessageStatistics(DateTime startTime, DateTime endTime);
        int GetTotalMessageCount(string clientEndpoint = null);

        Task UpdateClientStatusAsync(string clientEndpoint, bool isConnected);
        Task<List<RCS_TcpClientStatuses>> GetClientStatusesAsync();
        Task<List<RCS_TcpClientMessages>> GetMessageHistoryAsync(string clientEndpoint, int count = 100);

        /// <summary>
        /// 获取消息处理状态统计
        /// </summary>
        Task<MessageStatistics> GetMessageStatisticsAsync();

        /// <summary>
        /// 获取最近的消息列表
        /// </summary>
        /// <param name="count">获取的消息数量</param>
        Task<List<RCS_TcpClientMessages>> GetRecentMessagesAsync(int count);

        /// <summary>
        /// 重试处理失败的消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        Task RetryMessageAsync(int messageId);

        /// <summary>
        /// 获取分页消息列表
        /// </summary>
        Task<List<RCS_TcpClientMessages>> GetPagedMessagesAsync(int page, int pageSize);

        /// <summary>
        /// 获取消息总数
        /// </summary>
        Task<int> GetTotalMessageCountAsync();
    }
}
