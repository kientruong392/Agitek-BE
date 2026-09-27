using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class OrderTrackingLog
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Order Order { get; set; } = null!;
        public Location? Location { get; set; }
    }
}
