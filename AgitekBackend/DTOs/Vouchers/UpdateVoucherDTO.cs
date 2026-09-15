using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Vouchers
{
    public class UpdateVoucherDTO
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public int Quantity { get; set; }
        public int UsedCount { get; set; }
        public int PerUserLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VoucherStatus Status { get; set; }
    }
}


