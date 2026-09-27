using AgitekBackend.Common;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; }
        [Range(1, 9999)]
        public int InitialQuantity { get; set; }
        [Range(1, 9999)]
        public int Quantity { get; set; }
        public int UsedCount { get; set; } = 0;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public VoucherStatus Status { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}