using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Refund
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int UserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Response { get; set; }
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public OrderDetail OrderDetail { get; set; } = null!;
        public User? User { get; set; }
        public User? ApprovedByUser { get; set; }
        public ICollection<Media> Medias { get; set; } = new List<Media>();
    }
}
