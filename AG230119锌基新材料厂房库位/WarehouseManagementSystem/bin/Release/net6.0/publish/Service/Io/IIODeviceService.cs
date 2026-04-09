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
        Task<RCS_IODevices> GetDeviceByIdAsync(int id);
        Task<RCS_IODevices> AddDeviceAsync(RCS_IODevices device);
        Task UpdateDeviceAsync(RCS_IODevices device);
        Task DeleteDeviceAsync(int id);
        Task DeleteSignAsync(int id);
        Task<bool> ConnectDeviceAsync(string ip);

        Task<int> AddSignalAsync(RCS_IOSignals signal);

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
        private readonly IIOService _ioService;

        public IODeviceService(IDatabaseService db, ILogger<IODeviceService> logger, IIOService ioService)
        {
            _db = db;
            _logger = logger;
            _ioService = ioService;
        }

        public async Task<List<RCS_IODevices>> GetAllDevicesAsync()
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

        public async Task<RCS_IODevices> GetDeviceByIdAsync(int id)
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

        public async Task<RCS_IODevices> AddDeviceAsync(RCS_IODevices device)
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

        public async Task UpdateDeviceAsync(RCS_IODevices device)
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

        public async Task DeleteDeviceAsync(int id)
        {
            using var conn = _db.CreateConnection();
            // 先删除设备下的所有信号
            await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE DeviceId = @DeviceId", new { DeviceId = id });
            // 再删除设备
            await conn.ExecuteAsync("DELETE FROM RCS_IODevices WHERE Id = @Id", new { Id = id });
        }

        public async Task DeleteSignAsync(int id)
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> ConnectDeviceAsync(string ip)
        {
            return await _ioService.Conn(ip);
        }

        public async Task<int> AddSignalAsync(RCS_IOSignals signal)
        {
            using var conn = _db.CreateConnection();
            signal.CreatedTime = DateTime.Now;
            signal.UpdatedTime = DateTime.Now;

            return await conn.QuerySingleAsync<int>(@"
            INSERT INTO RCS_IOSignals (DeviceId, Name, Address, Description, CreatedTime, UpdatedTime)
            VALUES (@DeviceId, @Name, @Address, @Description, @CreatedTime, @UpdatedTime);
            SELECT CAST(SCOPE_IDENTITY() as int)", signal);
        }

        public async Task UpdateSignalAsync(RCS_IOSignals signal)
        {
            using var conn = _db.CreateConnection();
            signal.UpdatedTime = DateTime.Now;

            await conn.ExecuteAsync(@"
            UPDATE RCS_IOSignals 
            SET Name = @Name,
                Address = @Address,
                Description = @Description,
                UpdatedTime = @UpdatedTime
            WHERE Id = @Id", signal);
        }

        public async Task DeleteSignalAsync(int id)
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM RCS_IOSignals WHERE Id = @Id", new { Id = id });
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
                throw;
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
                throw;
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
                throw;
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
                throw;
            }
        }

    }
}
