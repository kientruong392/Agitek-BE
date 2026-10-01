using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Length(6,20)]
        public string? Username { get; set; }
        [MaxLength(320)]
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public List<Provider> Providers { get; set; } = new List<Provider>();
        public RoleName Role { get; set; } = RoleName.Customer;
        public int? GrantedByUserId { get; set; }
        public DateTime? RoleGrantedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }
        public User? GrantedByUser { get; set; }
        public Profile Profile { get; set; } = new Profile();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Product> CreatedProducts { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Order> ApprovedOrders { get; set; } = new List<Order>();
        public ICollection<Order> DeliveredOrders { get; set; } = new List<Order>();
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public ICollection<Refund> ApprovedRefunds { get; set; } = new List<Refund>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Review> RepliedReviews { get; set; } = new List<Review>();
        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
        public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
