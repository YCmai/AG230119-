using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models.IO
{
    // Models/Requests/WriteSignalRequest.cs
    public class WriteSignalRequest
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "无效的IP地址格式")]
        public string IP { get; set; }

        public string Address { get; set; }

        [Required]
        public bool Value { get; set; }
    }

    // Models/Requests/AddDeviceRequest.cs
    public class AddDeviceRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "设备名称长度不能超过100个字符")]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "无效的IP地址格式")]
        public string IP { get; set; }

        public bool IsEnabled { get; set; } = true;
    }

    // Models/Requests/AddSignalRequest.cs
    public class AddSignalRequest
    {
        [Required]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "信号名称长度不能超过100个字符")]
        public string Name { get; set; }

        [Required]
        [Range(0, 65535, ErrorMessage = "地址必须在0-65535范围内")]
        public int Address { get; set; }

        [StringLength(200, ErrorMessage = "描述长度不能超过200个字符")]
        public string Description { get; set; }
    }

    // Models/Responses/SignalStatusResponse.cs
    public class SignalStatusResponse
    {
        public bool Success { get; set; }
        public bool Value { get; set; }
        public string Message { get; set; }
    }

    // Models/Responses/ApiResponse.cs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
