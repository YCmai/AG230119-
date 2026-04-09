namespace WarehouseManagementSystem.Models
{
    /// <summary>
    /// 消息处理状态统计
    /// </summary>
    public class MessageStatistics
    {
        /// <summary>
        /// 待处理消息数量
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 处理中消息数量
        /// </summary>
        public int ProcessingCount { get; set; }

        /// <summary>
        /// 已完成消息数量
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 失败消息数量
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 总消息数量
        /// </summary>
        public int TotalCount => PendingCount + ProcessingCount + CompletedCount + FailedCount;

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatisticsTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreatedBy { get; set; } = "YCmai";
    }
}
