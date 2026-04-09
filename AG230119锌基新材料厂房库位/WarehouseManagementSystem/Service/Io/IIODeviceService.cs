using System.Net;

using Dapper;

using Microsoft.EntityFrameworkCore;

using WarehouseManagementSystem.Db;
using WarehouseManagementSystem.Models.IO;

namespace WarehouseManagementSystem.Service.Io
{
    // Services/IIODeviceService.cs
    public interface IIODeviceService
    {
        Task<List<RCS_IODevices>> GetAllDevicesAsync();

        Task<List<RCS_IOSignals>> GetLatestSignalsAsync();

        Task<List<RCS_IOAGV_Tasks>> GetRCS_IOAGV_TasksAsync();

        Task<RCS_IODevices> GetDeviceByIdAsync(int id);
        Task<RCS_IODevices> AddDeviceAsync(RCS_IODevices device);
        Task UpdateDeviceAsync(RCS_IODevices device);
        Task DeleteDeviceAsync(int id);
        Task DeleteSignAsync(int id);
      

        Task<int> AddSignalAsync(RCS_IOSignals signal);

        Task UpdateSignalAsync(RCS_IOSignals signal);

        Task<RCS_IODevices> GetDeviceByNameAsync(string name);
        Task<RCS_IODevices> GetDeviceByIPAsync(string ip);
        Task<RCS_IOSignals> GetSignalByNameAndDeviceAsync(int deviceId, string name);
        Task<RCS_IOSignals> GetSignalByAddressAndDeviceAsync(int deviceId, string address);
    }

    // Services/IODeviceService.cs
    // Services/IO/IODeviceService.cs
    public class IODeviceService : IIODeviceService
    {
        private readonly IDatabaseService _db;
        private readonly ILogger<IODeviceService> _logger;
       private readonly IServiceProvider _serviceProvider; 

        public IODeviceService(IDatabaseService db, ILogger<IODeviceService> logger, IServiceProvider serviceProvider)
        {
            _db = db;
            _logger = logger;
             _serviceProvider = serviceProvider;
        }

        public async Task<List<RCS_IODevices>> GetAllDevicesAsync()
        {
            try
            {
                using var conn = _db.CreateConnection();
                var devices = await conn.QueryAsync<RCS_IODevices>(@"
                SELECT Id, IP, Name, IsEnabled, CreatedTime, UpdatedTime 
                FROM RCS_IODevices");

                var signals = await conn.QueryAsync<RCS_IOSignals>(@"
                SELECT Id, DeviceId, Name, Address, Description, CreatedTime, UpdatedTime 
                FROM RCS_IOSignals");

                var deviceList = devices.ToList();
                var signalDict = signals.GroupBy(s => s.DeviceId)
                                       .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var device in deviceList)
                {
                    if (signalDict.TryGetValue(device.Id, out var deviceSignals))
                    {
                        device.Signals = deviceSignals;
                    }
                    else
                    {
                        device.Signals = new List<RCS_IOSignals>();
                    }
                }

                return deviceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有设备列表失败");
                return new List<RCS_IODevices>(); // 返回空列表而不是抛出异常
            }
        }

        public async Task<RCS_IODevices> GetDeviceByIdAsync(int id)
        {
            try
            {
                using var conn = _db.CreateConnection();
                var device = await conn.QueryFirstOrDefaultAsync<RCS_IODevices>(@"
                SELECT Id, IP, Name, IsEnabled, CreatedTime, UpdatedTime 
                FROM RCS_IODevices 
                WHERE Id = @Id", new { Id = id });

                if (device != null)
                {
                    device.Signals = (await conn.QueryAsync<RCS_IOSignals>(@"
                    SELECT Id, DeviceId, Name, Address, Description, CreatedTime, UpdatedTime 
                    FROM RCS_IOSignals 
                    WHERE DeviceId = @DeviceId", new { DeviceId = id })).ToList();
                }

                return device;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据ID获取设备失败: {id}");
                return null; // 返回null而不是抛出异常
            }
        }

        public async Task<RCS_IODevices> AddDeviceAsync(RCS_IODevices device)
        {
            try
            {
                using var conn = _db.CreateConnection();
                device.CreatedTime = DateTime.Now;
                device.UpdatedTime = DateTime.Now;

                var id = await conn.QuerySingleAsync<int>(@"
                INSERT INTO RCS_IODevices (IP, Name, IsEnabled, CreatedTime, UpdatedTime)
                VALUES (@IP, @Name, @IsEnabled, @CreatedTime, @UpdatedTime);
                SELECT CAST(SCOPE_IDENTITY() as int)", device);

                device.Id = id;
                return device;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加设备失败: {@Device}", device);
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

        public async Task UpdateDeviceAsync(RCS_IODevices device)
        {
            try
            {
                using var conn = _db.CreateConnection();
                device.UpdatedTime = DateTime.Now;

                await conn.ExecuteAsync(@"
                UPDATE RCS_IODevices 
                SET IP = @IP,
                    Name = @Name,
                    IsEnabled = @IsEnabled,
                    UpdatedTime = @UpdatedTime
                WHERE Id = @Id", device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备失败: {@Device}", device);
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

        public async Task DeleteDeviceAsync(int id)
        {
            try
            {
                using var conn = _db.CreateConnection();
                // 先删除设备下的所有信号
                await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE DeviceId = @DeviceId", new { DeviceId = id });
                // 再删除设备
                await conn.ExecuteAsync("DELETE FROM RCS_IODevices WHERE Id = @Id", new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除设备失败: {id}");
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

        public async Task DeleteSignAsync(int id)
        {
            try
            {
                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE Id = @Id", new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除信号失败: {id}");
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

       
        public async Task<int> AddSignalAsync(RCS_IOSignals signal)
        {
            try
            {
                using var conn = _db.CreateConnection();
                signal.CreatedTime = DateTime.Now;
                signal.UpdatedTime = DateTime.Now;

                return await conn.QuerySingleAsync<int>(@"
                INSERT INTO RCS_IOSignals (DeviceId, Name, Address, Description, CreatedTime, UpdatedTime,Value)
                VALUES (@DeviceId, @Name, @Address, @Description, @CreatedTime, @UpdatedTime,@Value);
                SELECT CAST(SCOPE_IDENTITY() as int)", signal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加信号失败: {@Signal}", signal);
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

        public async Task UpdateSignalAsync(RCS_IOSignals signal)
        {
            try
            {
                _logger.LogInformation("执行数据更新-开始");

                using var conn = _db.CreateConnection();

                await conn.ExecuteAsync(@"
                UPDATE RCS_IOSignals 
                SET Value = @Value,
                    UpdatedTime = @UpdatedTime
                WHERE Id = @Id", signal);

                _logger.LogInformation("执行数据更新-完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSignalAsync失败: {@Signal}", signal);
            }
        }

        public async Task DeleteSignalAsync(int id)
        {
            try
            {
                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE Id = @Id", new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除信号失败: {id}");
                throw; // 这里抛出异常，因为调用者需要知道操作是否成功
            }
        }

        // 根据设备名称查询
        public async Task<RCS_IODevices> GetDeviceByNameAsync(string name)
        {
            const string sql = @"
            SELECT * FROM RCS_IODevices 
            WHERE Name = @name";

            try
            {
                using var conn = _db.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<RCS_IODevices>(sql, new { name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据名称查询设备失败: {name}", name);
                return null; // 返回null而不是抛出异常
            }
        }

        // 根据IP地址查询设备
        public async Task<RCS_IODevices> GetDeviceByIPAsync(string ip)
        {
            const string sql = @"
            SELECT * FROM RCS_IODevices 
            WHERE IP = @ip";

            try
            {
                using var conn = _db.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<RCS_IODevices>(sql, new { ip });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据IP查询设备失败: {ip}", ip);
                return null; // 返回null而不是抛出异常
            }
        }

        // 根据设备ID和信号名称查询信号
        public async Task<RCS_IOSignals> GetSignalByNameAndDeviceAsync(int deviceId, string name)
        {
            const string sql = @"
            SELECT * FROM RCS_IOSignals 
            WHERE DeviceId = @deviceId 
            AND Name = @name";

            try
            {
                using var conn = _db.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<RCS_IOSignals>(sql, new { deviceId, name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据设备ID和名称查询信号失败: DeviceId={deviceId}, Name={name}", deviceId, name);
                return null; // 返回null而不是抛出异常
            }
        }

        // 根据设备ID和地址查询信号
        public async Task<RCS_IOSignals> GetSignalByAddressAndDeviceAsync(int deviceId, string address)
        {
            const string sql = @"
            SELECT * FROM RCS_IOSignals 
            WHERE DeviceId = @deviceId 
            AND Address = @address";

            try
            {
                using var conn = _db.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<RCS_IOSignals>(sql, new { deviceId, address });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据设备ID和地址查询信号失败: DeviceId={deviceId}, Address={address}", deviceId, address);
                return null; // 返回null而不是抛出异常
            }
        }

        public async Task<List<RCS_IOAGV_Tasks>> GetRCS_IOAGV_TasksAsync()
        {
            try
            {
                using var conn = _db.CreateConnection();
                var tasks = await conn.QueryAsync<RCS_IOAGV_Tasks>(
                    @"SELECT TOP 15 * 
                    FROM RCS_IOAGV_Tasks 
                    ORDER BY CreatedTime DESC");

                return tasks.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取IO任务列表失败");
                return new List<RCS_IOAGV_Tasks>(); // 返回空列表而不是抛出异常
            }
        }

        public async Task<List<RCS_IOSignals>> GetLatestSignalsAsync()
        {
            try
            {
                using var conn = _db.CreateConnection();
                
                // 首先获取所有启用的设备
                var devices = await conn.QueryAsync<RCS_IODevices>(@"
                    SELECT Id, IP, Name, IsEnabled, CreatedTime, UpdatedTime 
                    FROM RCS_IODevices 
                    WHERE IsEnabled = 1");

                // 获取这些设备的所有信号
                var signals = await conn.QueryAsync<RCS_IOSignals>(@"
                    SELECT s.Id, s.DeviceId, s.Name, s.Address, s.Description, 
                           s.CreatedTime, s.UpdatedTime, s.Value
                    FROM RCS_IOSignals s
                    INNER JOIN RCS_IODevices d ON s.DeviceId = d.Id
                    WHERE d.IsEnabled = 1");

                // 按设备分组并构建设备-信号关系
                var deviceList = devices.ToList();
                var signalDict = signals.GroupBy(s => s.DeviceId)
                                       .ToDictionary(g => g.Key, g => g.ToList());

                // 将信号关联到对应的设备
                foreach (var device in deviceList)
                {
                    if (signalDict.TryGetValue(device.Id, out var deviceSignals))
                    {
                        device.Signals = deviceSignals;
                    }
                    else
                    {
                        device.Signals = new List<RCS_IOSignals>();
                    }
                }

                // 返回所有信号
                return signals.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新信号列表失败");
                return new List<RCS_IOSignals>(); // 返回空列表而不是抛出异常
            }
        }
    }
}
