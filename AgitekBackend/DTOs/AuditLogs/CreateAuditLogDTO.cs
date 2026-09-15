namespace AgitekBackend.DTOs.AuditLogs
{
    public class CreateAuditLogDTO
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}


