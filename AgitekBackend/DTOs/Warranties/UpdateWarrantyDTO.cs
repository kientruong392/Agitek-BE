using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Warranties
{
    public class UpdateWarrantyDTO
    {
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ProductCondition { get; set; } = string.Empty;
        public WarrantyStatus Status { get; set; }
        public string? RejectReason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}


