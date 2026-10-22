using AgitekBackend.Common;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;

namespace AgitekBackend.DTOs.Warranties
{
    public class WarrantyDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProductCondition { get; set; } = string.Empty;
        public WarrantyStatus Status { get; set; }
        public string? Response { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OrderDetailDTO? OrderDetail { get; set; }
        public OrderDTO? Order { get; set; }
        public UserDTO? User { get; set; }
        public List<MediaDTO> Medias { get; set; } = new();
    }
}
