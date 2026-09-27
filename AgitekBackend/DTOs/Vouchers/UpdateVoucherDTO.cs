using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Vouchers
{
    public class UpdateVoucherDTO
    {
        [MaxLength(100, ErrorMessage = "Voucher name cannot exceed 100 characters")]
        public string? Name { get; set; }
        [MaxLength(500, ErrorMessage = "Voucher description cannot exceed 500 characters")]
        public string? Description { get; set; }
        [Range(1, 9999, ErrorMessage = "Quantity must be between 1 and 9999")]
        public int? Quantity { get; set; }
        public VoucherStatus? Status { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
