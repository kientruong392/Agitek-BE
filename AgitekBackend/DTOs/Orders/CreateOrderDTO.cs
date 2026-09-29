using AgitekBackend.Common;
using AgitekBackend.DTOs.OrderDetails;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Orders
{
    public class CreateOrderDTO
    {
        public int? VoucherId { get; set; }
        [Required(ErrorMessage = "Address Id cannot be empty")]
        public int AddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
        public List<CreateOrderDetailDTO> OrderDetails { get; set; } = new();
    }
}