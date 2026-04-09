namespace WarehouseManagementSystem.Models.PLC
{
    /// <summary>
    /// PLC交互记录实体
    /// </summary>
    public class PlcInteraction
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// PLC地址ID
        /// </summary>
        public int PlcAddressId { get; set; }

        /// <summary>
        /// 操作类型：0:写入, 1:读取, 2:监控, 3:报警
        /// </summary>
        public OperationType? OperationType { get; set; }

        /// <summary>
        /// 原值
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        public string OperatorName { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 最后重试时间
        /// </summary>
        public DateTime? LastRetryTime { get; set; }

        /// <summary>
        /// 交互来源：0:手动操作, 1:自动任务, 2:定时任务, 3:联动触发
        /// </summary>
        public InteractionSource? Source { get; set; }

        /// <summary>
        /// 优先级：0:低, 1:中, 2:高
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 关联的PLC地址信息
        /// </summary>
        public virtual PlcAddress PlcAddress { get; set; }
    }


    /// <summary>
    /// 交互来源
    /// </summary>
    /// <summary>
    /// 交互来源枚举
    /// </summary>
    public enum InteractionSource
    {
        /// <summary>
        /// 手动操作（通过页面操作）
        /// </summary>
        Manual = 0,

        /// <summary>
        /// 自动任务（由系统自动执行）
        /// </summary>
        AutoTask = 1,

        /// <summary>
        /// 定时任务
        /// </summary>
        Schedule = 2,

        /// <summary>
        /// 监控任务（状态监控）
        /// </summary>
        Monitor = 3,

        /// <summary>
        /// 报警触发
        /// </summary>
        Alarm = 4,

        /// <summary>
        /// 外部系统调用（如MES、WMS等）
        /// </summary>
        External = 5,

        /// <summary>
        /// API接口调用
        /// </summary>
        Api = 6,

        /// <summary>
        /// 系统初始化
        /// </summary>
        Initialize = 7,

        /// <summary>
        /// 系统恢复
        /// </summary>
        Recovery = 8,

        /// <summary>
        /// 联动触发
        /// </summary>
        Trigger = 9,

        /// <summary>
        /// 设备反馈
        /// </summary>
        DeviceFeedback = 10,

        /// <summary>
        /// 其他来源
        /// </summary>
        Other = 99
    }
}
