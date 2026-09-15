using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        [Required, MaxLength(50)]
        public string EntityName { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}
