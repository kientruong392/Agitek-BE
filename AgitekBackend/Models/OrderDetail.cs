using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AgitekBackend.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
        public Refund? Refund { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
