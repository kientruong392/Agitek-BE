using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Products
{
    public class CreateProductDTO
    {
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
        public int WarrantyPeriod { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UserId { get; set; }
    }
}


