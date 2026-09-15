using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string SKU { get; set; } = string.Empty;
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(300)]
        public string Slug { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; } = 0;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public int WarrantyPeriod { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }
        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public User? User { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}