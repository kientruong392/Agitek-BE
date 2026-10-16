using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Payments
{
    public class CreatePaymentDTO
    {
        [Required(ErrorMessage = "Order Id cannot be empty")]
        public int OrderId { get; set; }
        [Required(ErrorMessage = "Payment method cannot be empty")]
        public PaymentMethod PaymentMethod { get; set; }
        [MaxLength(100, ErrorMessage = "Transaction Id cannot exceed 100 characters")]
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
    }
}
