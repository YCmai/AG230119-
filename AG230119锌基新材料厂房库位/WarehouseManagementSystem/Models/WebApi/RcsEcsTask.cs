using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models.WebApi
{
    public class RcsEcsTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestCode { get; set; }

        [Required]
        [StringLength(50)]
        public string TaskCode { get; set; }

        public int TaskStatus { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public bool Execute { get; set; }

        public string ExecuteResult { get; set; }

        public int RetryCount { get; set; }
    }
}
