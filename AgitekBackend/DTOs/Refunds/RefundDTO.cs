using AgitekBackend.Common;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.OrderDetails;

namespace AgitekBackend.DTOs.Refunds
{
    public class RefundDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int UserId { get; set; }
        public int? ApprovedByUserId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Response { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OrderDetailDTO? OrderDetail { get; set; }
        public ICollection<MediaDTO> Medias { get; set; } = new List<MediaDTO>();
    }
}
