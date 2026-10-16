namespace AgitekBackend.DTOs.Vouchers
{
    public class VoucherApplicationDTO
    {
        public int VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}
