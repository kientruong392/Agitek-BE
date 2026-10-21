namespace AgitekBackend.DTOs.OrderTrackingLogs
{
    public class OrderTrackingLogDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? LocationId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}


