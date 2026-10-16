using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Payments
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderUserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? BankId { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? TransferDescription { get; set; }
    }
}