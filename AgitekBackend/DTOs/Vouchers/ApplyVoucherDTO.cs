using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Vouchers
{
    public class ApplyVoucherDTO
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(0, (double)decimal.MaxValue)]
        public decimal SubtotalPrice { get; set; }
    }
}
