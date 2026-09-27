namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class UpdateOrderTrackingLogDTO
    {
        public int? OrderId { get; set; }
        public int? LocationId { get; set; }
        public string? Description { get; set; } 
        public DateTime? Timestamp { get; set; }
    }
}


