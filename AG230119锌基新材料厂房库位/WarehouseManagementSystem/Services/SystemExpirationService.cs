using Microsoft.Extensions.Configuration;

namespace WarehouseManagementSystem.Services
{
    public interface ISystemExpirationService
    {
        bool IsSystemExpired();
        DateTime? GetExpirationDate();
        int GetRemainingDays();
    }

    public class SystemExpirationService : ISystemExpirationService
    {
        private readonly IConfiguration _configuration;
        private DateTime? _expirationDate;

        public SystemExpirationService(IConfiguration configuration)
        {
            _configuration = configuration;
            string expirationDateString = _configuration["SystemAccess:ExpirationDate"];
            
            if (!string.IsNullOrEmpty(expirationDateString) && 
                DateTime.TryParse(expirationDateString, out var expirationDate))
            {
                _expirationDate = expirationDate;
            }
        }

        public bool IsSystemExpired()
        {
            if (!_expirationDate.HasValue)
                return false;

            return DateTime.Now > _expirationDate.Value;
        }

        public DateTime? GetExpirationDate()
        {
            return _expirationDate;
        }

        public int GetRemainingDays()
        {
            if (!_expirationDate.HasValue)
                return -1;

            TimeSpan remaining = _expirationDate.Value - DateTime.Now;
            return remaining.TotalDays > 0 ? (int)remaining.TotalDays : 0;
        }
    }
} 