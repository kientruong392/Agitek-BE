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

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DiscountType DiscountType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0;
        public int Quantity { get; set; }
        public int UsedCount { get; set; } = 0;
        public int PerUserLimit { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } 
        public VoucherStatus Status { get; set; } = VoucherStatus.Active;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}