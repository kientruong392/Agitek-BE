using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Payments
{
    public class UpdatePaymentDTO
    {
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}