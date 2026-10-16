namespace AgitekBackend.DTOs.OrderDetails
{
    public class OrderDetailDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string ProductSlug { get; set; } = string.Empty;
        public bool HasReview { get; set; }
    }
}


