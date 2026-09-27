using AgitekBackend.Common;
using AgitekBackend.DTOs.Orders;

namespace AgitekBackend.DTOs.Vouchers
{
    public class VoucherDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public int InitialQuantity { get; set; }
        public int Quantity { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VoucherStatus Status { get; set; }
    }
}
