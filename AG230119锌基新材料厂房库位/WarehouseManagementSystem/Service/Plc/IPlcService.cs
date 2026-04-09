using WarehouseManagementSystem.Models.PLC;

public interface IPlcService
{
    Task<PagedResult<PlcAddress>> GetPlcAddressesAsync(int pageNumber, int pageSize, string groupName = "");
    Task<PagedResult<PlcInteraction>> GetPlcInteractionsAsync(int pageNumber, int pageSize);
    Task<bool> WriteValueAsync(int addressId, string value, string operatorId, string operatorName);
    Task<bool> ResetSignalAsync(int addressId, string operatorId, string operatorName);
    Task<IEnumerable<PlcInteraction>> GetUnexecutedInteractionsAsync();
   
    Task UpdateInteractionStatusAsync(int interactionId, bool isSuccess, string errorMessage);

    Task UpdatePlcAddressValueAsync(int addressId, string newValue);

    /// <summary>
    /// 获取需要监控的PLC地址列表
    /// </summary>
    Task<IEnumerable<PlcAddress>> GetMonitoringAddressesAsync();

    /// <summary>
    /// 添加PLC历史数据记录
    /// </summary>
    Task AddPlcHistoryAsync(int addressId, string value);

    /// <summary>
    /// 添加PLC交互记录
    /// </summary>
    Task AddPlcInteractionAsync(PlcInteraction interaction);
    Task DeletePlcInteractionAsync(int id);
    Task ClearPlcInteractionsAsync();

}