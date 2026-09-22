namespace AgitekBackend.DTOs.AuditLogs
{
    public class UpdateAuditLogDTO
    {
        public DateTime? Timestamp { get; set; }
        public int? UserId { get; set; }
        public string? EntityName { get; set; } 
        public string? Action { get; set; } 
    }
}


