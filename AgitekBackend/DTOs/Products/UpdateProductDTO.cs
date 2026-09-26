using AgitekBackend.Common;
using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.DTOs.ProductSpecifications;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Products
{
    public class UpdateProductDTO
    {
        [MaxLength(16, ErrorMessage = "Product code cannot exceed 16 characters")]
        public string? ProductCode { get; set; }
        [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string? ProductName { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Original price must be a valid decimal")]
        public decimal? OriginalPrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Discount price must be a valid decimal")]
        public decimal? DiscountPrice { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a valid integer")]
        public string? Description { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Warranty period must be a valid integer")]
        public int? WarrantyPeriod { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<UpdateProductSpecificationDTO>? Specifications { get; set; } = new();
        public List<UpdateProductImageDTO>? Images { get; set; } = new();
    }
}


