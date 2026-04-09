namespace WarehouseManagementSystem.Models.PLC
{
    /// <summary>
    /// PLC地址配置实体
    /// </summary>
    public class PlcAddress
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 地址名称，例如："电磁线进空箱"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// PLC地址，例如："DM30300"
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 当前值（字符串格式存储）
        /// </summary>
        public string CurrentValue { get; set; }

        /// <summary>
        /// 设备站号
        /// </summary>
        public string StationNumber { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// PLC的IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// PLC的端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 等待点
        /// </summary>
        public string WattingNode { get; set; }
    }

    /// <summary>
    /// 访问模式
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        /// 只读
        /// </summary>
        ReadOnly = 0,

        /// <summary>
        /// 只写
        /// </summary>
        WriteOnly = 1,

        /// <summary>
        /// 读写
        /// </summary>
        ReadWrite = 2
    }
}
