using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class CreateOrderTrackingLogDTO
    {
        [Required(ErrorMessage = "Warehouse cannot be empty")]
        [Range(1, int.MaxValue)]
        public int? LocationId { get; set; }
        [Required(ErrorMessage = "Description cannot be empty")]
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
