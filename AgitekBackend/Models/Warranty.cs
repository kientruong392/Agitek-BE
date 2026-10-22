using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Warranty
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        [Required, MaxLength(1000)]
        public string ProductCondition { get; set; } = string.Empty;
        public WarrantyStatus Status { get; set; } = WarrantyStatus.Pending;
        [MaxLength(500)]
        public string? Response { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public OrderDetail? OrderDetail { get; set; }
        public User? User { get; set; }
        public ICollection<Media> Media { get; set; } = new List<Media>();
    }
}
