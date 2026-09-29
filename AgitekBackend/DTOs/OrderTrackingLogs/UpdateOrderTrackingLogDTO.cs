using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class UpdateOrderTrackingLogDTO
    {
        public int? OrderId { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
