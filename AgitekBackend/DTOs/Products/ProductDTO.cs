using AgitekBackend.Common;
using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.DTOs.ProductSpecifications;

namespace AgitekBackend.DTOs.Products
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int InitialQuantity { get; set; }
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public string? Description { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
        public int WarrantyPeriod { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<ProductSpecificationDTO> Specifications { get; set; } = new();
        public List<ProductImageDTO> Images { get; set; } = new();
    }
}


