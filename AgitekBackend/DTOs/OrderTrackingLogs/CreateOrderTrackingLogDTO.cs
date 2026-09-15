namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class CreateOrderTrackingLogDTO
    {
        public int OrderId { get; set; }
        public int? LocationCoordinatesId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}


