public enum PlcType
{
    Siemens = 1,    // 西门子
    Mitsubishi = 2, // 三菱
    Omron = 3       // 欧姆龙
}

public enum PlcDataType
{
    Bool = 0,
    Int16 = 1,
    Int32 = 2,
    Float = 3,
    String = 4
}

/// <summary>
/// 操作类型枚举
/// </summary>
public enum OperationType
{
    /// <summary>
    /// 写入信号
    /// </summary>
    Write = 0,

    /// <summary>
    /// 读取信号
    /// </summary>
    Read = 1,

    /// <summary>
    /// 复位信号
    /// </summary>
    Reset = 2,

    /// <summary>
    /// 报警触发
    /// </summary>
    Alarm = 3,

    /// <summary>
    /// 自动监控
    /// </summary>
    Monitor = 4,

    /// <summary>
    /// 心跳信号
    /// </summary>
    Heartbeat = 5,

    /// <summary>
    /// 启动信号
    /// </summary>
    Start = 6,

    /// <summary>
    /// 停止信号
    /// </summary>
    Stop = 7,

    /// <summary>
    /// 暂停信号
    /// </summary>
    Pause = 8,

    /// <summary>
    /// 继续信号
    /// </summary>
    Resume = 9,

    /// <summary>
    /// 急停信号
    /// </summary>
    EmergencyStop = 10,

    /// <summary>
    /// 确认信号
    /// </summary>
    Acknowledge = 11,

    /// <summary>
    /// 取消信号
    /// </summary>
    Cancel = 12,

    /// <summary>
    /// 自动模式
    /// </summary>
    AutoMode = 13,

    /// <summary>
    /// 手动模式
    /// </summary>
    ManualMode = 14,

    /// <summary>
    /// 报警复位
    /// </summary>
    AlarmReset = 15,

    /// <summary>
    /// 系统初始化
    /// </summary>
    Initialize = 16,

    /// <summary>
    /// 状态更新
    /// </summary>
    StatusUpdate = 17,

    /// <summary>
    /// 参数设置
    /// </summary>
    ParameterSet = 18,

    /// <summary>
    /// 数据采集
    /// </summary>
    DataCollection = 19,

    /// <summary>
    /// 其他操作
    /// </summary>
    Other = 99
}