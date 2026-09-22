using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Products
{
    public class UpdateProductDTO
    {
        public string? SKU { get; set; } 
        public string? Name { get; set; } 
        public string? Slug { get; set; } 
        public decimal? OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? StockQuantity { get; set; }
        public string? Description { get; set; }
        public ProductStatus? Status { get; set; }
        public int? WarrantyPeriod { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? UserId { get; set; }
    }
}


