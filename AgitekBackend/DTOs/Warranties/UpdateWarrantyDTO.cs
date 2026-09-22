using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Warranties
{
    public class UpdateWarrantyDTO
    {
        public string? Code { get; set; } 
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? UserId { get; set; }
        public string? FullName { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string? Address { get; set; } 
        public string? ProductCondition { get; set; } 
        public WarrantyStatus? Status { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}


