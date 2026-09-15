namespace AgitekBackend.DTOs.AuditLogs
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}


