using WarehouseManagementSystem.Models.WebApi;

namespace WarehouseManagementSystem.Service.WepApi
{
    public interface IWebApiTaskService
    {
        Task<PagedResult<RcsEcsTask>> GetPagedTasksAsync(int pageIndex, int pageSize);
        Task<IEnumerable<RcsEcsTask>> GetUnexecutedTasksAsync();
        Task UpdateTaskAsync(RcsEcsTask task);
    }
}
