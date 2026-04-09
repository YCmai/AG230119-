using AciModule.Domain.Entitys;
using AciModule.Domain.Shared;
using Spark.Domain.Entitys;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Spark.Domain.Worker;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Auditing;
using System.IO.Pipes;
using Newtonsoft.Json.Linq;
using WMS.LineCallInputModule.Domain;
using Rcs.Domain.Entitys;
using Spark.Application;
using Microsoft.Extensions.Caching.Memory;

namespace Rcs.Application
{
    [Route("api/Agv")]
    [DisableAuditing]
    public class ControlAppService:ApplicationService
    {
        private readonly IRepository<NdcTask_Moves, Guid> _ndcTaskRepos;
        private readonly IRepository<RCS_WmsTask> _wmmTasks;
        private static LoggerManager _loggerManager;
        private readonly IMemoryCache _cache;

        public ControlAppService(IRepository<NdcTask_Moves, Guid> ndcTaskRepos, IMemoryCache cache, LoggerManager loggerManager, IRepository<RCS_WmsTask> wmmTasks

            )
        {
            _ndcTaskRepos = ndcTaskRepos;
            _wmmTasks = wmmTasks;
            _loggerManager = loggerManager;
            _cache = cache;
        }



        [HttpPost("CreateTask")]
        public async Task<ReturnEntity> CreateAgvTask(CreateAgvTaskEntity taskRequest)
        {
            try
            {
                // 记录接口调用日志
                await _loggerManager.LogAndLogCritical("接口 CreateTask 被调用。");

                // 检查是否存在相同任务编号的任务
                var existingTask = await _wmmTasks.FirstOrDefaultAsync(x => x.TaskCode == taskRequest.TaskCode);
                if (existingTask != null)
                {
                    await _loggerManager.LogAndLogError($"存在相同的任务编号 {taskRequest.TaskCode} 的任务。");
                    return new ReturnEntity
                    {
                        Status = 0,
                        Message = $"存在相同的任务编号 {taskRequest.TaskCode} 的任务，请修改后重新提交。"
                    };
                }

                // 创建新任务对象
                var newTask = new RCS_WmsTask
                {
                    CreateTime = DateTime.Now,
                    PickupHeight = taskRequest.PickupHeight,
                    PickupPoint = taskRequest.PickupPoint,
                    Priority = taskRequest.Priority,
                    TaskCode = taskRequest.TaskCode,
                    TaskStatus = 0, // 任务初始状态
                    TaskType = taskRequest.TaskType,
                    UnloadHeight = taskRequest.UnloadHeight,
                    UnloadPoint = taskRequest.UnloadPoint
                };

                // 插入新任务
                await _wmmTasks.InsertAsync(newTask);

                // 记录任务创建日志
                await _loggerManager.LogAndLogCritical($"收到 AGV 任务, 编号: {taskRequest.TaskCode}, 取料点: {taskRequest.PickupPoint}, 取料高度: {taskRequest.PickupHeight}, 卸料点: {taskRequest.UnloadPoint}, 卸料高度: {taskRequest.UnloadHeight}");

                return new ReturnEntity
                {
                    Status = 1,
                    Message = $"AGV 任务已下达, 取料点: {taskRequest.PickupPoint}, 高度: {taskRequest.PickupHeight}, 卸料点: {taskRequest.UnloadPoint}, 高度: {taskRequest.UnloadHeight}"
                };
            }
            catch (Exception ex)
            {
                // 记录异常日志
                await _loggerManager.LogAndLogError($"接口接收数据错误: {ex.Message}");
                return new ReturnEntity
                {
                    Status = 0,
                    Message = $"接口接收数据错误: {ex.Message}"
                };
            }
        }

        // 取消多余的任务
        [HttpPost("CancelTask")]
        public async Task<ReturnEntity> CancelTask(CancelRequest cancelRequest)
        {
            try
            {
                await _loggerManager.LogAndLogCritical("接口 CancelTask 被调用。");
                // 查找 NDC 任务
                var ndcTask = await _ndcTaskRepos.FirstOrDefaultAsync(i => i.SchedulTaskNo == cancelRequest.CancelCode);
                if (ndcTask != null)
                {
                    // 检查任务状态是否已取消
                    if (ndcTask.TaskStatus == TaskStatuEnum.Canceled)
                    {
                        return new ReturnEntity
                        {
                            Status = 0,
                            Message = "当前任务已被取消，无法重复操作"
                        };
                    }

                    // 设置任务为取消状态
                    ndcTask.CancelTask = true;
                    await _ndcTaskRepos.UpdateAsync(ndcTask);
                    return new ReturnEntity
                    {
                        Status = 1,
                        Message = $"任务号: {cancelRequest.CancelCode} 取消成功"
                    };
                }

                // 查找 WMS 任务
                var wmsTask = await _wmmTasks.FirstOrDefaultAsync(x => x.TaskCode == cancelRequest.CancelCode);
                if (wmsTask != null)
                {
                    // 更新任务状态为已取消
                    wmsTask.TaskStatus = TaskStatuEnum.Canceled;
                    await _wmmTasks.UpdateAsync(wmsTask);
                    return new ReturnEntity
                    {
                        Status = 1,
                        Message = $"任务号: {cancelRequest.CancelCode} 取消成功"
                    };
                }

                // 未找到任务
                return new ReturnEntity
                {
                    Status = 0,
                    Message = $"查询不到任务号: {cancelRequest.CancelCode} 的任务数据"
                };
            }
            catch (Exception ex)
            {
                // 记录错误日志
                await _loggerManager.LogAndLogError($"接口 CancelTask 请求失败: {ex}");
                return new ReturnEntity
                {
                    Status = 0,
                    Message = $"接口 CancelTask 请求失败: {ex.Message}"
                };
            }
        }




        [HttpPost("SelectTask")]
        public async Task<SelectData> SelectAgvTask([FromQuery] string taskCode)
        {
            // 构建缓存的key，可以根据 taskCode 或者上位机的IP等唯一标识
            var cacheKey = $"TaskCall_{taskCode}";

            // 检查缓存中是否存在记录，5秒内调用过的任务会被缓存
            if (_cache.TryGetValue(cacheKey, out _))
            {
                return new SelectData
                {
                    Status = 0,
                    Message = "请求过于频繁，请稍后再试"
                };
            }

            try
            {
                // 查询 NDC 任务
                var taskData = await _ndcTaskRepos.FirstOrDefaultAsync(i => i.SchedulTaskNo == taskCode);
                if (taskData == null)
                {
                    return new SelectData
                    {
                        Status = 0,
                        Message = $"查询不到任务号: {taskCode} 的任务数据"
                    };
                }

                // 将当前请求写入缓存，有效期为 5 秒
                _cache.Set(cacheKey, true, TimeSpan.FromSeconds(1));

                return new SelectData
                {
                    AgvCode = taskData.AgvId,
                    Status = 1,
                    Message = "查询成功",
                    TaskStatus = (int)taskData.TaskStatus
                };
            }
            catch (Exception ex)
            {
                await _loggerManager.LogAndLogCritical($"接口 SelectTask 错误: {ex.Message}");
                return new SelectData
                {
                    AgvCode = 0,
                    Status = 0,
                    Message = $"数据查询异常: {ex.Message}"
                };
            }
        }

    }

}
