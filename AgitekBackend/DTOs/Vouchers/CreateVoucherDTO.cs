using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Vouchers
{
    public class CreateVoucherDTO
    {
        [MaxLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string? Code { get; set; }
        [Required(ErrorMessage = "Voucher name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Voucher name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Voucher description cannot exceed 500 characters")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Discount type cannot be empty")]
        public DiscountType DiscountType { get; set; }
        [Required(ErrorMessage = "Discount value cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount value must be a valid decimal")]
        public decimal DiscountValue { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Max discount value must be a valid decimal")]
        public decimal? MaxDiscountValue { get; set; }
        [Required(ErrorMessage = "Min order value cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Min order value must be a valid decimal")]
        public decimal MinOrderValue { get; set; }
        [Required(ErrorMessage = "Initial quantity cannot be empty")]
        [Range(1, 9999, ErrorMessage = "Initial quantity must be between 1 and 9999")]
        public int InitialQuantity { get; set; }
        [Required(ErrorMessage = "Start date cannot be empty")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date cannot be empty")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Voucher status cannot be empty")]
        public VoucherStatus Status { get; set; }
    }
}


