using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Orders
{
    public class CreateOrderDTO
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
        public OrderStatus Status { get; set; } = OrderStatus.Ordered;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
    }
}


