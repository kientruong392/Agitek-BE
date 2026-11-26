using AgitekBackend.Common;
using AgitekBackend.DTOs.Locations;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.DTOs.OrderTrackingLogs;
using AgitekBackend.DTOs.Payments;

namespace AgitekBackend.DTOs.Orders
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int? PaymentId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? PaymentExpiresAt { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int? ApprovedByUserId { get; set; }
        public int? DeliveredByUserId { get; set; }
        public int? VoucherId { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal SubtotalPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public ShippingType ShippingType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public LocationDTO? FromLocation { get; set; }
        public LocationDTO? ToLocation { get; set; }
        public List<PaymentDTO> Payments { get; set; } = new();
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
        public List<OrderTrackingLogDTO> OrderTrackingLogs { get; set; } = new();
        public List<MediaDTO> Medias { get; set; } = new();
    }
}
