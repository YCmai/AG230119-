using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models
{
    public class SystemManagementViewModel
    {
        [Display(Name = "系统到期日期")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }
        
        [Display(Name = "剩余天数")]
        public int RemainingDays { get; set; }
    }
} 