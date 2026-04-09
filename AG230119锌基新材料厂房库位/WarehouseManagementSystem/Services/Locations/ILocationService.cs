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
using Microsoft.EntityFrameworkCore;
using System.Linq;

public interface ILocationService
{
    Task<(IEnumerable<RCS_Locations> Items, int TotalItems)> GetLocations(string searchString = "", int page = 1, int pageSize = 10);

    Task<(IEnumerable<RCS_Locations> Items, int TotalCount)> GetSearchLocations(string searchString, int page, int pageSize);

    Task<RCS_Locations> GetLocationById(int id);

    // 添加按分组获取储位的方法
    Task<List<RCS_Locations>> GetLocationsByGroup(string group);

    Task<(bool Success, string Message)> CreateOrUpdateLocation(RCS_Locations location);
    Task<(bool Success, string Message)> HandleLocationOperation(int id, int type);
    Task<(int Available, int Used)> GetStorageCapacityStats();
    Task<(List<RCS_Locations> Items, int TotalItems, int Available, int Used)> GetLocationsWithStats(string searchString = "", int page = 1);

    // 按区域批量清空物料
    Task<(bool success, string message, int affectedCount)> BatchClearMaterials(string group);

    // 按区域批量锁定/解锁储位
    Task<(bool success, string message, int affectedCount)> BatchToggleLock(string group, bool lockState);

    // 按ID列表批量清空物料
    Task<(bool success, string message, int affectedCount)> BatchClearMaterialsByIds(List<int> locationIds);

    // 按ID列表批量锁定/解锁储位
    Task<(bool success, string message, int affectedCount)> BatchToggleLockByIds(List<int> locationIds, bool lockState);

    // 按区域批量设置数量（置满/置空）
    Task<(bool success, string message, int affectedCount)> BatchSetQuantityByGroup(string group, string quantity);

    // 按ID列表批量设置数量（置满/置空）
    Task<(bool success, string message, int affectedCount)> BatchSetQuantityByIds(List<int> locationIds, string quantity);


    Task<(bool success, string message, int affectedCount)> BatchUpdateMaterialCode(List<int> locationIds, string newMaterialCode);

    // 按区域批量清空物料编号
    Task<(bool success, string message, int affectedCount)> BatchClearMaterialCodeByGroup(string group);
}

public class LocationService : ILocationService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<LocationService> _logger;

    public LocationService(IDatabaseService db, ILogger<LocationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 获取NodeRemark的第index段数字
    private int GetNodeRemarkPart(string nodeRemark, int index)
    {
        if (string.IsNullOrEmpty(nodeRemark)) return 0;
        var parts = nodeRemark.Split('-');
        if (index < parts.Length && int.TryParse(parts[index], out var n))
            return n;
        return 0;
    }

    public async Task<(bool success, string message, int affectedCount)> BatchUpdateMaterialCode(List<int> locationIds, string newMaterialCode)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            if (locationIds == null || !locationIds.Any())
            {
                return (false, "请选择要修改的库位", 0);
            }

            if (string.IsNullOrWhiteSpace(newMaterialCode))
            {
                return (false, "新物料编号不能为空", 0);
            }

            // 更新指定ID的储位物料编号
            int affectedCount = await connection.ExecuteAsync(@"
                UPDATE RCS_Locations
                SET MaterialCode = @NewMaterialCode
                WHERE Id IN @LocationIds",
                new { LocationIds = locationIds, NewMaterialCode = newMaterialCode },
                transaction);

            transaction.Commit();
            return (true, $"成功修改 {affectedCount} 个储位的物料编号", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "批量修改物料编号失败");
            return (false, "批量修改失败：" + ex.Message, 0);
        }
    }


    public async Task<(bool success, string message, int affectedCount)> BatchClearMaterialsByIds(List<int> locationIds)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 清空指定ID的储位物料信息
            int affectedCount = await connection.ExecuteAsync(@"
            UPDATE RCS_Locations
            SET MaterialCode = NULL,
                PalletID = '0',
                Weight = '0',
                Quanitity = '0',
                EntryDate = NULL
            WHERE Id IN @LocationIds",
                new { LocationIds = locationIds },
                transaction);

            transaction.Commit();
            return (true, $"成功清空 {affectedCount} 个储位的物料", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "批量清空储位物料失败");
            return (false, "清空物料失败，请稍后再试", 0);
        }
    }

    public async Task<(bool success, string message, int affectedCount)> BatchToggleLockByIds(List<int> locationIds, bool lockState)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 执行批量锁定/解锁操作
            string operation = lockState ? "锁定" : "解锁";

            int affectedCount = await connection.ExecuteAsync(@"
            UPDATE RCS_Locations
            SET Lock = @LockState
            WHERE Id IN @LocationIds
            AND Lock <> @LockState",
                new
                {
                    LocationIds = locationIds,
                    LockState = lockState ? 1 : 0
                },
                transaction);

            transaction.Commit();
            return (true, $"成功{operation} {affectedCount} 个储位", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            string operation = lockState ? "锁定" : "解锁";
            _logger.LogError(ex, $"批量{operation}储位失败");
            return (false, $"批量{operation}失败，请稍后再试", 0);
        }
    }


    // NodeRemark排序Key
    private class NodeRemarkSortKey : IComparable<NodeRemarkSortKey>
    {
        public int Type; // 0=中文, 1=数字编号
        public List<int> Segments;
        public string Raw;

        public int CompareTo(NodeRemarkSortKey other)
        {
            if (Type != other.Type) return Type.CompareTo(other.Type);
            if (Type == 0) // 中文，按字符串
                return string.Compare(Raw, other.Raw, StringComparison.Ordinal);
            // 数字编号，按自然顺序
            int minLen = Math.Min(Segments.Count, other.Segments.Count);
            for (int i = 0; i < minLen; i++)
            {
                int cmp = Segments[i].CompareTo(other.Segments[i]);
                if (cmp != 0) return cmp;
            }
            return Segments.Count.CompareTo(other.Segments.Count);
        }
    }

    private NodeRemarkSortKey GetNodeRemarkSortKey(string nodeRemark)
    {
        if (string.IsNullOrEmpty(nodeRemark)) return new NodeRemarkSortKey { Type = 0, Raw = "" };
        
        // 只包含数字和-的才算数字编号
        if (System.Text.RegularExpressions.Regex.IsMatch(nodeRemark, @"^\d+(?:-\d+)*$"))
        {
            // 确保正确解析每个数字段
            var segs = nodeRemark.Split('-')
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .ToList();
                
            return new NodeRemarkSortKey { Type = 1, Segments = segs, Raw = nodeRemark };
        }
        
        // 其它情况（带中文）排最前
        return new NodeRemarkSortKey { Type = 0, Raw = nodeRemark };
    }

    public async Task<(IEnumerable<RCS_Locations> Items, int TotalCount)> GetSearchLocations(string searchString, int page, int pageSize)
    {
        using var connection = _db.CreateConnection();

        try
        {
            // 构建基础查询
            var whereClause = string.IsNullOrEmpty(searchString)
                ? ""
                : "WHERE NodeRemark LIKE @Search OR MaterialCode LIKE @Search OR Name LIKE @Search";

            // 获取总记录数
            var countSql = $"SELECT COUNT(*) FROM RCS_Locations {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql,
                new { Search = $"%{searchString}%" });

            // 获取所有数据，不在SQL中排序
            var sql = $@"SELECT * FROM RCS_Locations {whereClause}";

            var items = await connection.QueryAsync<RCS_Locations>(sql, new
            {
                Search = $"%{searchString}%"
            });

            // 先按Group分组，再按NodeRemark进行自然排序
            var result = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return (result, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位列表失败");
            throw;
        }
    }

    public async Task<(IEnumerable<RCS_Locations> Items, int TotalItems)> GetLocations(string searchString = "", int page = 1, int pageSize = 10)
    {
        try
        {
            using var conn = _db.CreateConnection();

            var query = "SELECT * FROM RCS_Locations";
            var countQuery = "SELECT COUNT(*) FROM RCS_Locations";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " WHERE NodeRemark LIKE @Search";
                countQuery += " WHERE NodeRemark LIKE @Search";
                parameters.Add("@Search", $"%{searchString}%");
            }

            query += " ORDER BY [Group], NodeRemark OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("@Offset", (page - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            var items = await conn.QueryAsync<RCS_Locations>(query, parameters);
            var totalItems = await conn.ExecuteScalarAsync<int>(countQuery, parameters);

            // 多级排序，假设NodeRemark格式为A-B-C
            return (items
                .OrderBy(x => GetNodeRemarkPart(x.NodeRemark, 0))
                .ThenBy(x => GetNodeRemarkPart(x.NodeRemark, 1))
                .ThenBy(x => GetNodeRemarkPart(x.NodeRemark, 2)),
                totalItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位列表失败");
            throw;
        }
    }

    public async Task<(bool success, string message, int affectedCount)> BatchClearMaterials(string group)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 找出指定分组中的所有有物料的储位
            //var locationsWithMaterials = await connection.QueryAsync<RCS_Locations>(@"
            //    SELECT * FROM RCS_Locations
            //    WHERE [Group] = @Group 
            //    AND MaterialCode IS NOT NULL 
            //    AND MaterialCode <> ''",
            //    new { Group = group },
            //    transaction);

            //if (!locationsWithMaterials.Any())
            //{
            //    transaction.Commit();
            //    return (true, $"区域 {group} 没有需要清空的物料", 0);
            //}

            // 清空这些储位的物料信息
            int affectedCount = await connection.ExecuteAsync(@"
                UPDATE RCS_Locations
                SET MaterialCode = NULL,
                    PalletID = '0',
                    Weight = '0',
                    Quanitity = '',
                    EntryDate = NULL
                WHERE [Group] = @Group 
                AND MaterialCode IS NOT NULL 
                AND MaterialCode <> ''",
                new { Group = group },
                transaction);

            transaction.Commit();
            return (true, $"成功清空区域 {group} 中的 {affectedCount} 个储位物料", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, $"批量清空区域 {group} 物料失败");
            return (false, "清空物料失败，请稍后再试", 0);
        }
    }

        public async Task<(bool success, string message, int affectedCount)> BatchToggleLock(string group, bool lockState)
        {
            using var connection = _db.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 执行批量锁定/解锁操作
                string operation = lockState ? "锁定" : "解锁";

                // 只操作与目标状态不同的储位
                int affectedCount = await connection.ExecuteAsync(@"
                    UPDATE RCS_Locations
                    SET Lock = @LockState
                    WHERE [Group] = @Group 
                    AND Lock <> @LockState",
                    new { Group = group, LockState = lockState ? 1 : 0 },
                    transaction);

                transaction.Commit();
                return (true, $"成功{operation}区域 {group} 中的 {affectedCount} 个储位", affectedCount);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                string operation = lockState ? "锁定" : "解锁";
                _logger.LogError(ex, $"批量{operation}区域 {group} 储位失败");
                return (false, $"批量{operation}失败，请稍后再试", 0);
            }
        }



    public async Task<RCS_Locations> GetLocationById(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
            "SELECT * FROM RCS_Locations WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<(bool Success, string Message)> CreateOrUpdateLocation(RCS_Locations location)
    {
        try
        {
            using var conn = _db.CreateConnection();

            if (location.Id == 0)
            {
                var existing = await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
                    "SELECT * FROM RCS_Locations WHERE NodeRemark = @NodeRemark",
                    new { location.NodeRemark });

                if (existing != null)
                {
                    return (false, "保存失败，库位置点重复！");
                }

                var sql = @"INSERT INTO RCS_Locations 
                    (Name, NodeRemark, MaterialCode, PalletID, Weight, Quanitity, 
                     EntryDate, [Group], LiftingHeight, Lock, WattingNode) 
                    VALUES 
                    (@Name, @NodeRemark, @MaterialCode, @PalletID, @Weight, @Quanitity, 
                     @EntryDate, @Group, @LiftingHeight, @Lock, @WattingNode)";

                await conn.ExecuteAsync(sql, location);
                return (true, "新存储位置已成功创建！");
            }
            else
            {
                var existing = await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
                    "SELECT * FROM RCS_Locations WHERE Id = @Id",
                    new { location.Id });

                if (existing == null)
                {
                    return (false, "找不到要更新的储位");
                }

                var sql = @"UPDATE RCS_Locations 
                    SET Name = @Name, NodeRemark = @NodeRemark, MaterialCode = @MaterialCode, 
                        PalletID = @PalletID, Weight = @Weight, Quanitity = @Quanitity, 
                        EntryDate = @EntryDate, [Group] = @Group, LiftingHeight = @LiftingHeight, 
                        Lock = @Lock, WattingNode = @WattingNode 
                    WHERE Id = @Id";

                await conn.ExecuteAsync(sql, location);
                return (true, "修改成功");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存库位信息失败");
            throw;
        }
    }

    public async Task<(bool Success, string Message)> HandleLocationOperation(int id, int type)
    {
        try
        {
            using var conn = _db.CreateConnection();
            var location = await conn.QueryFirstOrDefaultAsync<RCS_Locations>(
                "SELECT * FROM RCS_Locations WHERE Id = @Id",
                new { Id = id });

            if (location == null)
            {
                return (false, "操作失败，找不到该储位。");
            }

            switch (type)
            {
                case 1: // 删除
                    await conn.ExecuteAsync("DELETE FROM RCS_Locations WHERE Id = @Id", new { Id = id });
                    return (true, "储位删除成功！");

                case 2: // 解锁
                    await conn.ExecuteAsync(
                        "UPDATE RCS_Locations SET Lock = @Lock WHERE Id = @Id",
                        new { Id = id, Lock = !location.Lock });
                    return (true, "储位信息修改成功！");

                case 3: // 清空物料
                    await conn.ExecuteAsync(@"
                        UPDATE RCS_Locations 
                        SET MaterialCode = NULL, PalletID = NULL, Weight = '0', Quanitity = '0' 
                        WHERE Id = @Id", new { Id = id });
                    return (true, "物料清空成功！");

                case 4: // 重置异常物料
                    if (location.MaterialCode != null && location.MaterialCode.StartsWith("Err_"))
                    {
                        await conn.ExecuteAsync(@"
                            UPDATE RCS_Locations 
                            SET MaterialCode = @MaterialCode 
                            WHERE Id = @Id",
                            new { Id = id, MaterialCode = location.MaterialCode.Replace("Err_", "") });
                        return (true, "异常物料重置成功！");
                    }
                    return (false, "该储位不包含异常物料！");

                default:
                    return (false, "无效的操作类型！");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作失败");
            throw;
        }
    }

    public async Task<(int Available, int Used)> GetStorageCapacityStats()
    {
        try
        {
            using var conn = _db.CreateConnection();
            var locations = await conn.QueryAsync<RCS_Locations>("SELECT MaterialCode FROM RCS_Locations");
            
            var used = locations.Count(loc => 
                !string.IsNullOrEmpty(loc.MaterialCode) && loc.MaterialCode != "empty");
            var total = locations.Count();

            return (total, used);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取存储容量统计失败");
            throw;
        }
    }

    public async Task<(List<RCS_Locations> Items, int TotalItems, int Available, int Used)> GetLocationsWithStats(string searchString = "", int page = 1)
    {
        try
        {
            using var conn = _db.CreateConnection();
            const int pageSize = 15; // 修改为每页显示15条数据

            // 构建查询
            var query = "SELECT * FROM RCS_Locations";
            var countQuery = "SELECT COUNT(*) FROM RCS_Locations";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " WHERE NodeRemark LIKE @Search";
                countQuery += " WHERE NodeRemark LIKE @Search";
                parameters.Add("@Search", $"%{searchString}%");
            }

            // 添加分页
            query += " ORDER BY [Group], NodeRemark OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            parameters.Add("@Offset", (page - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            // 执行查询
            var items = await conn.QueryAsync<RCS_Locations>(query, parameters);
            var totalItems = await conn.ExecuteScalarAsync<int>(countQuery, parameters);

            // 获取统计信息
            var allLocations = await conn.QueryAsync<RCS_Locations>("SELECT MaterialCode FROM RCS_Locations");
            var used = allLocations.Count(loc => 
                !string.IsNullOrEmpty(loc.MaterialCode) && loc.MaterialCode != "empty");
            var total = allLocations.Count();

            return (items.ToList(), totalItems, total, used);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库位列表和统计信息失败");
            throw;
        }
    }

    public async Task<(bool success, string message, int affectedCount)> BatchSetQuantityByGroup(string group, string quantity)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            int affectedCount = await connection.ExecuteAsync(@"
                UPDATE RCS_Locations
                SET Quanitity = @Quantity
                WHERE [Group] = @Group",
                new { Group = group, Quantity = quantity }, transaction);
            transaction.Commit();
            return (true, $"成功设置区域 {group} 中的 {affectedCount} 个储位数量为{quantity}", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, $"批量设置区域 {group} 数量失败");
            return (false, "批量设置数量失败，请稍后再试", 0);
        }
    }

    public async Task<(bool success, string message, int affectedCount)> BatchSetQuantityByIds(List<int> locationIds, string quantity)
    {
        var maCode = "";

        if (quantity=="满")
        {
            maCode = "满";
        }

        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            int affectedCount = await connection.ExecuteAsync(@"
                UPDATE RCS_Locations
                SET Quanitity = @Quantity,MaterialCode = @MaterialCode
                WHERE Id IN @LocationIds",
                new { LocationIds = locationIds, Quantity = quantity, MaterialCode  = maCode }, transaction);
            transaction.Commit();
            return (true, $"成功设置 {affectedCount} 个储位数量为{quantity}", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, $"批量设置储位数量失败");
            return (false, "批量设置数量失败，请稍后再试", 0);
        }
    }

    public async Task<List<RCS_Locations>> GetLocationsByGroup(string group)
    {
        try
        {
            using var conn = _db.CreateConnection();
            var locations = await conn.QueryAsync<RCS_Locations>(
                "SELECT * FROM RCS_Locations WHERE [Group] = @Group",
                new { Group = group });
            return locations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取分组 {group} 的储位失败");
            throw;
        }
    }

    public async Task<(bool success, string message, int affectedCount)> BatchClearMaterialCodeByGroup(string group)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 清空指定分组中的物料编号
            int affectedCount = await connection.ExecuteAsync(@"
                UPDATE RCS_Locations
                SET MaterialCode = NULL,Quanitity = 0
                WHERE [Group] = @Group",
                new { Group = group },
                transaction);

            transaction.Commit();
            return (true, $"成功清空区域 {group} 中的 {affectedCount} 个储位物料编号", affectedCount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, $"批量清空区域 {group} 物料编号失败");
            return (false, "清空物料编号失败，请稍后再试", 0);
        }
    }
} 