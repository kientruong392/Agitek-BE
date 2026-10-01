using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; } 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } 
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        [MaxLength(100)]
        public string? TransactionId { get; set; }
        [MaxLength(500)]
        public string? GatewayResponse { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public Order Order { get; set; } = null!;
    }
}