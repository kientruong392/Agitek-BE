namespace AgitekBackend.DTOs.AuditLogs
{
    public class AuditLogQueryDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? EntityName { get; set; }
        public string? Action { get; set; }
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Search { get; set; }
    }

    public class AuditLogPageDTO
    {
        public List<AuditLogDTO> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class StockReceiptDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
