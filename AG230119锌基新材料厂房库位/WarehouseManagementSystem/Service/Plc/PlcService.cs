using HslCommunication;
using Microsoft.Data.SqlClient;
using WarehouseManagementSystem.Models.PLC;
using Dapper;
using WarehouseManagementSystem.Db;

namespace WarehouseManagementSystem.Service.Plc
{
    public class PlcService : IPlcService
    {
        private readonly ILogger<PlcService> _logger;
        private readonly IDatabaseService _db;

        public PlcService(
            ILogger<PlcService> logger,
            IDatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<PagedResult<PlcAddress>> GetPlcAddressesAsync(int pageNumber, int pageSize, string groupName = "")
        {
            try
            {
                using var connection = _db.CreateConnection();

                var sql = string.Empty;
                object parameters;

                if (string.IsNullOrEmpty(groupName))
                {
                    sql = @"
                SELECT COUNT(*) FROM RCS_PlcAddresses;
                SELECT * FROM RCS_PlcAddresses 
                ORDER BY Id
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

                    parameters = new
                    {
                        Offset = (pageNumber - 1) * pageSize,
                        PageSize = pageSize
                    };
                }
                else
                {
                    sql = @"
                SELECT COUNT(*) 
                FROM RCS_PlcAddresses 
                WHERE Name LIKE @GroupPattern;

                SELECT * FROM RCS_PlcAddresses 
                WHERE Name LIKE @GroupPattern
                ORDER BY Id
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

                    parameters = new
                    {
                        GroupPattern = groupName + "-%",
                        Offset = (pageNumber - 1) * pageSize,
                        PageSize = pageSize
                    };
                }

                using var multi = await connection.QueryMultipleAsync(sql, parameters);

                var totalItems = await multi.ReadFirstAsync<int>();
                var items = (await multi.ReadAsync<PlcAddress>()).ToList();

                return new PagedResult<PlcAddress>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取PLC地址列表失败");
                throw;
            }
        }

        public async Task<IEnumerable<PlcAddress>> GetMonitoringAddressesAsync()
        {
            try
            {
                using var connection = _db.CreateConnection();
                var sql = @"
            SELECT Id, Name, Address, CurrentValue, StationNumber, 
                   UpdateTime, IpAddress, Port
            FROM RCS_PlcAddresses
            ORDER BY IpAddress, Port";

                return await connection.QueryAsync<PlcAddress>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取监控地址列表失败");
                throw;
            }
        }

        public async Task AddPlcHistoryAsync(int addressId, string value)
        {
            try
            {
                using var connection = _db.CreateConnection();
                var sql = @"
            INSERT INTO RCS_PlcHistory (PlcAddressId, Value, CreateTime)
            VALUES (@AddressId, @Value, @CreateTime)";

                await connection.ExecuteAsync(sql, new
                {
                    AddressId = addressId,
                    Value = value,
                    CreateTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"添加PLC历史数据失败 - 地址ID: {addressId}");
                throw;
            }
        }

        public async Task AddPlcInteractionAsync(PlcInteraction interaction)
        {
            try
            {
                using var connection = _db.CreateConnection();
                var sql = @"
            INSERT INTO RCS_PlcInteractions 
            (PlcAddressId, OperationType, OldValue, NewValue, IsSuccess, 
             CreateTime, Source, Priority, Remarks)
            VALUES 
            (@PlcAddressId, @OperationType, @OldValue, @NewValue, @IsSuccess,
             @CreateTime, @Source, @Priority, @Remarks)";

                await connection.ExecuteAsync(sql, interaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加PLC交互记录失败");
                throw;
            }
        }




        public async Task<PagedResult<PlcInteraction>> GetPlcInteractionsAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var connection = _db.CreateConnection();

                // 获取总记录数
                var totalItems = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM RCS_PlcInteractions");

                // 获取分页数据
                var sql = @"
                SELECT i.*, a.* 
                FROM RCS_PlcInteractions i
                LEFT JOIN RCS_PlcAddresses a ON i.PlcAddressId = a.Id
                ORDER BY i.CreateTime DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

                var result = await connection.QueryAsync<dynamic>(sql, new
                {
                    Offset = (pageNumber - 1) * pageSize,
                    PageSize = pageSize
                });

                var interactions = result.Select(row => new PlcInteraction
                {
                    Id = row.Id,
                    PlcAddressId = row.PlcAddressId,
                    OperationType = row.OperationType == null ? null : (OperationType?)row.OperationType,
                    OldValue = row.OldValue,
                    NewValue = row.NewValue,
                    IsSuccess = row.IsSuccess,
                    ErrorMessage = row.ErrorMessage,
                    CreateTime = row.CreateTime,
                    Source = row.Source == null ? null : (InteractionSource?)row.Source,
                    OperatorId = row.OperatorId,
                    OperatorName = row.OperatorName,
                    Remarks = row.Remarks,
                    PlcAddress = row.Id == null ? null : new PlcAddress
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Address = row.Address
                    }
                }).OrderByDescending(x=>x.CreateTime).ToList();

                return new PagedResult<PlcInteraction>
                {
                    Items = interactions,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取PLC交互记录失败");
                throw;
            }
        }


        public async Task<IEnumerable<PlcInteraction>> GetUnexecutedInteractionsAsync()
        {
            try
            {
                using var connection = _db.CreateConnection();
                var sql = @"
            SELECT i.*, a.* 
            FROM RCS_PlcInteractions i
            LEFT JOIN RCS_PlcAddresses a ON i.PlcAddressId = a.Id
            WHERE i.IsSuccess = 0"; // 只选择未执行的交互记录

                return await connection.QueryAsync<PlcInteraction, PlcAddress, PlcInteraction>(
                    sql,
                    (interaction, address) =>
                    {
                        interaction.PlcAddress = address;
                        return interaction;
                    },
                    splitOn: "Id" // 指定分割字段
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取未执行的PLC交互任务失败");
                throw;
            }
        }

        public async Task<bool> WriteValueAsync(int addressId, string value, string operatorId, string operatorName)
        {
            try
            {
                using var connection = _db.CreateConnection();

                // 获取当前PLC地址信息
                var plcAddress = await connection.QueryFirstOrDefaultAsync<PlcAddress>(
                    "SELECT * FROM RCS_PlcAddresses WHERE Id = @Id",
                    new { Id = addressId });

                if (plcAddress == null) return false;

                // 检查是否为PLC写入地址（通过地址格式判断）
                if (!plcAddress.Address.EndsWith("200") && 
                    !plcAddress.Address.EndsWith("400") && 
                    !plcAddress.Address.EndsWith("600") && 
                    !plcAddress.Address.EndsWith("800"))
                {
                    _logger.LogWarning($"尝试写入PLC地址被拒绝 - 地址ID: {addressId}, 地址: {plcAddress.Address}");
                    throw new InvalidOperationException("只允许写入AGV信号，PLC信号为只读");
                }

                // 创建交互记录
                var interaction = new PlcInteraction
                {
                    PlcAddressId = addressId,
                    OperationType = OperationType.Write,
                    OldValue = plcAddress.CurrentValue,
                    NewValue = value,
                    IsSuccess = false, // 初始状态为未执行
                    CreateTime = DateTime.Now,
                    OperatorId = operatorId,
                    OperatorName = operatorName,
                    Source = InteractionSource.Manual,
                    Priority = 1 // 普通优先级
                };

                // 插入交互记录
                var sql = @"
                    INSERT INTO RCS_PlcInteractions 
                    (PlcAddressId, OperationType, OldValue, NewValue, IsSuccess, 
                     CreateTime, OperatorId, OperatorName, Source, Priority) 
                    VALUES 
                    (@PlcAddressId, @OperationType, @OldValue, @NewValue, @IsSuccess,
                     @CreateTime, @OperatorId, @OperatorName, @Source, @Priority)";

                await connection.ExecuteAsync(sql, interaction);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"写入PLC地址被拒绝 - 地址ID: {addressId}");
                throw; // 重新抛出异常，让控制器处理
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建PLC写入请求失败 - 地址ID: {addressId}, 新值: {value}");
                throw;
            }
        }

        public async Task UpdateInteractionStatusAsync(int interactionId, bool isSuccess, string errorMessage)
        {
            try
            {
                using var connection = _db.CreateConnection();
                await connection.ExecuteAsync(@"
            UPDATE RCS_PlcInteractions 
            SET IsSuccess = @IsSuccess, 
                ErrorMessage = @ErrorMessage
            WHERE Id = @Id",
                    new { Id = interactionId, IsSuccess = isSuccess, ErrorMessage = errorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新PLC交互状态失败");
                throw;
            }
        }


        public async Task<bool> ResetSignalAsync(int addressId, string operatorId, string operatorName)
        {
            try
            {
                using var connection = _db.CreateConnection();

                var plcAddress = await connection.QueryFirstOrDefaultAsync<PlcAddress>(
                    "SELECT * FROM RCS_PlcAddresses WHERE Id = @Id",
                    new { Id = addressId });

                if (plcAddress == null) return false;

                var interaction = new PlcInteraction
                {
                    PlcAddressId = addressId,
                    OperationType = OperationType.Reset,
                    OldValue = plcAddress.CurrentValue,
                    NewValue = "false",
                    IsSuccess = false,
                    CreateTime = DateTime.Now,
                    OperatorId = operatorId,
                    OperatorName = operatorName,
                    Source = InteractionSource.Manual,
                    Priority = 1
                };

                var sql = @"
            INSERT INTO RCS_PlcInteractions 
            (PlcAddressId, OperationType, OldValue, NewValue, IsSuccess, 
             CreateTime, OperatorId, OperatorName, Source, Priority) 
            VALUES 
            (@PlcAddressId, @OperationType, @OldValue, @NewValue, @IsSuccess,
             @CreateTime, @OperatorId, @OperatorName, @Source, @Priority)";

                await connection.ExecuteAsync(sql, interaction);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建PLC复位请求失败 - 地址ID: {addressId}");
                throw;
            }
        }

        public async Task UpdatePlcAddressValueAsync(int addressId, string newValue)
        {
            try
            {
                using var connection = _db.CreateConnection();

                var sql = @"
                UPDATE RCS_PlcAddresses 
                SET CurrentValue = @NewValue,
                    UpdateTime = @UpdateTime
                WHERE Id = @AddressId";

                await connection.ExecuteAsync(sql, new
                {
                    AddressId = addressId,
                    NewValue = newValue,
                    UpdateTime = DateTime.Now
                });

                //_logger.LogInformation($"更新PLC地址值成功 - 地址ID: {addressId}, 新值: {newValue}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新PLC地址值失败 - 地址ID: {addressId}, 新值: {newValue}");
                throw;
            }
        }

      

        public async Task DeletePlcInteractionAsync(int id)
        {
            try
            {
                using var connection = _db.CreateConnection();

                // 首先检查记录是否存在
                var exists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM RCS_PlcInteractions WHERE Id = @Id",
                    new { Id = id });

                if (!exists)
                {
                    throw new InvalidOperationException($"交互记录 {id} 不存在");
                }

                // 删除记录
                var sql = "DELETE FROM RCS_PlcInteractions WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });

                _logger.LogInformation($"成功删除PLC交互记录 {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除PLC交互记录失败 - ID: {id}");
                throw;
            }
        }

        public async Task ClearPlcInteractionsAsync()
        {
            try
            {
                using var connection = _db.CreateConnection();

                // 保留最近24小时的记录，删除其他所有记录
                var sql = @"
            DELETE FROM RCS_PlcInteractions 
            WHERE CreateTime < @Threshold
            AND IsSuccess = 1";  // 只删除已完成的记录

                var result = await connection.ExecuteAsync(sql, new
                {
                    Threshold = DateTime.Now.AddHours(-24)
                });

                _logger.LogInformation($"成功清空PLC交互记录，共删除 {result} 条记录");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空PLC交互记录失败");
                throw;
            }
        }
    }
}
