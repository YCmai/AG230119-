using System.Net.Sockets;
using System.Net;
using NModbus;
using WarehouseManagementSystem.Hubs.TcpClient.Hubs;
using System.Data;
using WarehouseManagementSystem.Models.IO;
using WarehouseManagementSystem.Db;
using Dapper;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WarehouseManagementSystem.Hubs;
using System.Collections.Generic;
using System.Linq;
using System;

public interface ITaskGenerationService
{
    Task GenerateAGVTask(RCS_IOSignals signal, RCS_IODevices device);
}

public class TaskGenerationService : ITaskGenerationService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<TaskGenerationService> _logger;

    public TaskGenerationService(IDatabaseService db, ILogger<TaskGenerationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task GenerateAGVTask(RCS_IOSignals signal, RCS_IODevices device)
    {
        try
        {
            using var conn = _db.CreateConnection();
            
            // 1. 首先根据信号名称查找对应的库位信息
            var location = await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
                "SELECT * FROM RCS_Locations WHERE NodeRemark = @Name",
                new { Name = signal.Name });

            if (location == null)
            {
                _logger.LogWarning($"未找到信号{signal.Name}对应的库位信息");
                return;
            }

            // 2. 检查是否存在未完成的相同任务
            // 根据设备类型调整任务状态判断条件
            int maxTaskStatus = device.Id <= 27 ? 6 : 11;
            
            var existingTask = await conn.QueryFirstOrDefaultAsync<RCS_UserTasks>(
                @"SELECT * FROM RCS_UserTasks 
                WHERE sourcePosition = @SourcePosition 
                AND taskStatus < @MaxTaskStatus",
                new { SourcePosition = location.NodeRemark, MaxTaskStatus = maxTaskStatus });

            if (existingTask != null)
            {
                _logger.LogInformation($"已存在相同取货点的未完成任务，跳过任务生成");
                return;
            }

            // 3. 根据设备类型生成不同的任务
            if (device.Id <= 27) // 上料架任务
            {
                await GeneratePlatingToBufferTask(conn, location);
            }
            else if (device.Id >= 29) // 下料任务
            {
                _logger.LogInformation($"捕捉到{device.Id}的DI1信号闭合-2");

                await GenerateBufferToAssemblyTask(conn, location);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成AGV任务失败");
            throw;
        }
    }

    private async Task GeneratePlatingToBufferTask(IDbConnection conn, RCS_Locations sourceLocation)
    {
        // 查找锅炉起升架作为目标点
        var targetLocation = await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
            "SELECT * FROM RCS_Locations WHERE NodeRemark = @NodeRemark",
            new { NodeRemark = "起升架" });

        if (targetLocation == null)
        {
            _logger.LogError("未找到锅炉起升架位置信息");
            return;
        }

        await CreateTask(conn, sourceLocation, targetLocation, RCS_UserTasks.TaskType.上料);
    }

    private async Task GenerateBufferToAssemblyTask(IDbConnection conn, RCS_Locations sourceLocation)
    {
        try
        {
            // 创建新的数据库连接用于事务操作
            using var transactionConn = _db.CreateConnection();
            transactionConn.Open();

            // 开启数据库事务，确保所有操作要么全部成功，要么全部失败
            using var transaction = transactionConn.BeginTransaction();

            _logger.LogInformation($"捕捉到的DI1信号闭合-4");

            try
            {
                // 根据sourceLocation.NodeRemark决定使用哪条生产线
                string productLine = "1号线";  // 默认使用1号线
                if (sourceLocation.NodeRemark != null)
                {
                    if (sourceLocation.NodeRemark.Contains("2号") || sourceLocation.NodeRemark.Contains("取货2"))
                    {
                        productLine = "2号线";
                        _logger.LogInformation($"根据源位置 {sourceLocation.NodeRemark} 选择了2号线");
                    }
                    else
                    {
                        _logger.LogInformation($"根据源位置 {sourceLocation.NodeRemark} 选择了1号线");
                    }
                }
                
                _logger.LogInformation($"捕捉到的DI1信号闭合-5，准备查找{productLine}的可用储位");
                
                // 查询指定生产线的所有储位，按照NodeRemark排序
                var allLocations = await transactionConn.QueryAsync<RCS_Locations>(@"
                    SELECT * FROM RCS_Locations
                    WHERE [Group] LIKE @ProductLine
                    AND (Quanitity IS NULL OR Quanitity = '' OR Quanitity = '0') And (MaterialCode IS NULL OR MaterialCode = '')
                    AND Lock = 0
                    ORDER BY 
                        TRY_CAST(PARSENAME(REPLACE(NodeRemark, '-', '.'), 3) AS INT),
                        TRY_CAST(PARSENAME(REPLACE(NodeRemark, '-', '.'), 2) AS INT),
                        TRY_CAST(PARSENAME(REPLACE(NodeRemark, '-', '.'), 1) AS INT)",
                    new { ProductLine = $"%{productLine}%" },
                    transaction);
                
                // 转为列表，方便操作
                var locationsList = allLocations.ToList();
                _logger.LogInformation($"找到{locationsList.Count}个{productLine}可用（空闲且未锁定）储位");
                
                if (locationsList.Count == 0)
                {
                    _logger.LogError($"未找到{productLine}任何可用储位");
                    transaction.Rollback();
                    return;
                }
                
                // 按照NodeRemark自然顺序排列
                var sortedLocations = locationsList
                    .OrderBy(l => GetNodeRemarkPart(l.NodeRemark, 0))
                    .ThenBy(l => GetNodeRemarkPart(l.NodeRemark, 1))
                    .ThenBy(l => GetNodeRemarkPart(l.NodeRemark, 2))
                    .ToList();

                int GetNodeRemarkPart(string nodeRemark, int index)
                {
                    if (string.IsNullOrEmpty(nodeRemark)) return 0;
                    var parts = nodeRemark.Split('-');
                    if (index < parts.Length && int.TryParse(parts[index], out var n))
                        return n;
                    return 0;
                }
                
                // 遍历所有储位，按照策略寻找合适的储位
                RCS_Locations targetLocation = null;
                
                _logger.LogInformation($"开始按顺序检查所有储位");
                
                // 遍历所有储位
                foreach (var location in sortedLocations)
                {
                    // 检查是否为一层储位(NodeRemark以"-1"结尾)
                    if (location.NodeRemark.EndsWith("-1"))
                    {
                        // 直接使用一层储位
                        targetLocation = location;
                        _logger.LogInformation($"找到空闲的一层储位 {targetLocation.NodeRemark} 作为目标");
                        break;
                    }
                    // 检查是否为二层储位(NodeRemark以"-2"结尾)
                    else if (location.NodeRemark.EndsWith("-2"))
                    {
                        // 解析二层储位编号，例如 "A1-1-2" -> "A1-1-1"
                        var nodeRemarkParts = location.NodeRemark.Split('-');
                        if (nodeRemarkParts.Length >= 3)
                        {
                            // 构建对应的一层储位编号
                            string firstLevelRemark = $"{nodeRemarkParts[0]}-{nodeRemarkParts[1]}-1";
                            
                            _logger.LogInformation($"检查二层储位 {location.NodeRemark} 对应的一层储位 {firstLevelRemark}");
                            
                            // 查询对应的一层储位（已满且未锁定）
                            var firstLevel = await transactionConn.QueryFirstOrDefaultAsync<RCS_Locations>(@"
                                SELECT * FROM RCS_Locations
                                WHERE NodeRemark = @NodeRemark
                                AND (Quanitity = '满' OR Quanitity = '100')
                                AND Lock = 0",
                                new { NodeRemark = firstLevelRemark },
                                transaction);
                            
                            if (firstLevel != null)
                            {
                                // 找到了满足条件的二层储位（对应的一层满且未锁定）
                                targetLocation = location;
                                _logger.LogInformation($"选择二层储位 {targetLocation.NodeRemark} 作为目标，对应的一层 {firstLevelRemark} 已满且未锁定");
                                break;
                            }
                            else
                            {
                                _logger.LogInformation($"二层储位 {location.NodeRemark} 不满足条件，继续检查下一个");
                            }
                        }
                    }
                }
                
                _logger.LogInformation($"储位检查完成");
                
                // 如果没有找到合适的储位，报错并退出
                if (targetLocation == null)
                {
                    _logger.LogError($"{productLine}找不到可用的储位，或二层储位对应的一层不满或已锁定");
                    transaction.Rollback();
                    return;
                }
                
                // 锁定选中的储位
                await transactionConn.ExecuteAsync(@"
                    UPDATE RCS_Locations 
                    SET Lock = 1
                    WHERE Id = @Id",
                    new { Id = targetLocation.Id },
                    transaction);
                
                _logger.LogInformation($"捕捉到的DI1信号闭合-8");
                
                // 创建从缓冲区到装配线的AGV任务
                await CreateTask(transactionConn, sourceLocation, targetLocation, RCS_UserTasks.TaskType.下料, transaction);
                
                // 提交事务
                transaction.Commit();
                
                _logger.LogInformation($"成功锁定储位 {targetLocation.NodeRemark} 并创建任务");
            }
            catch (Exception ex)
            {
                // 发生异常时回滚事务
                transaction.Rollback();
                _logger.LogError(ex, "创建任务和锁定储位失败");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行GenerateBufferToAssemblyTask失败");
            throw;
        }
    }

    private async Task CreateTask(IDbConnection conn, RCS_Locations sourceLocation, RCS_Locations targetLocation, 
        RCS_UserTasks.TaskType taskType, IDbTransaction transaction = null)
    {
        var task = new RCS_UserTasks
        {
            taskStatus = TaskStatuEnum.None,
            creatTime = DateTime.Now,
            requestCode = $"{taskType}_{DateTime.Now:yyyyMMddHHmmss}_{sourceLocation.Name}",
            taskType = taskType,
            priority = 1,
            sourcePosition = sourceLocation.NodeRemark,
            targetPosition = targetLocation.NodeRemark,
            executed = false,
            IsCancelled = false,
        };

        await conn.ExecuteAsync(@"
            INSERT INTO RCS_UserTasks 
            (taskStatus, creatTime, requestCode, taskType, priority, sourcePosition, targetPosition, executed,IsCancelled) 
            VALUES 
            (@taskStatus, @creatTime, @requestCode, @taskType, @priority, @sourcePosition, @targetPosition, @executed,@IsCancelled)",
            task,
            transaction);

        _logger.LogInformation($"成功生成AGV任务：{sourceLocation.Name} -> {targetLocation.Name}");
    }
} 