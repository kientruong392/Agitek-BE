using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Orders
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Tracking number cannot be empty")]
        [MaxLength(20, ErrorMessage = "Tracking number cannot exceed 20 characters")]
        public string TrackingNumber { get; set; } = string.Empty;
        public int? VoucherId { get; set; }
        [Required(ErrorMessage = "Address cannot be empty")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "Subtotal price cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Subtotal price must be a valid decimal")]
        public decimal SubtotalPrice { get; set; }
        [Required(ErrorMessage = "Shipping fee cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Shipping fee must be a valid decimal")]
        public decimal ShippingFee { get; set; }
        [Required(ErrorMessage = "Discount amount cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be a valid decimal")]
        public decimal DiscountAmount { get; set; }
        [Required(ErrorMessage = "Total price cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Total price must be a valid decimal")]
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Ordered;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
    }
}


