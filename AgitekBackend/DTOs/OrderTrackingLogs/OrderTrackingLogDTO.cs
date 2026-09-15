namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class OrderTrackingLogDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? LocationCoordinatesId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}


