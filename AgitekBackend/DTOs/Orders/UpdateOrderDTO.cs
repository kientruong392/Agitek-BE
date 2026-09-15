using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Orders
{
    public class UpdateOrderDTO
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public int? DeliveredByUserId { get; set; }
        public int? VoucherId { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal SubtotalPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
    }
}


