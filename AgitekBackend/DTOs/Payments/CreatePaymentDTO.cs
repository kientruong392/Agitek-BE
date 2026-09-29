using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Payments
{
    public class CreatePaymentDTO
    {
        [Required(ErrorMessage = "Order Id cannot be empty")]
        public int OrderId { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        [MaxLength(100, ErrorMessage = "Transaction Id cannot exceed 100 characters")]
        public string? TransactionId { get; set; }
    }
}
