using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Media
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
        public int? ReviewId { get; set; }
        public int? WarrantyId { get; set; }
        public int? OrderId { get; set; }
        public int? RefundOrderId { get; set; }
        public int? RefundProductId { get; set; }
        public Review? Review { get; set; }
        public Warranty? Warranty { get; set; }
        public Order? Order { get; set; }
        public Refund? Refund { get; set; }
    }
}
