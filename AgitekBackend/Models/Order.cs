using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string TrackingNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public int? DeliveredByUserId { get; set; }
        public int? VoucherId { get; set; }
        public int? FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int? AddressId { get; set; }
        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubtotalPrice { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingFee { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Ordered;
        [MaxLength(50)]
        public string? TransactionId { get; set; }
        [MaxLength(255)]
        public string? Note { get; set; }
        [MaxLength(500)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public User User { get; set; } = null!;
        public User? ApprovedByUser { get; set; }
        public User? DeliveredByUser { get; set; }
        public Voucher? Voucher { get; set; }
        public Location? FromLocation { get; set; }
        public Location? ToLocation { get; set; }
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<OrderTrackingLog> OrderTrackingLogs { get; set; } = new List<OrderTrackingLog>();
        public ICollection<Media> Medias { get; set; } = new List<Media>();
    }
}
