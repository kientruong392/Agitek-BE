using AgitekBackend.Common;
using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.DTOs.ProductSpecifications;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.DTOs.Products
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product code cannot be empty")]
        [MaxLength(16, ErrorMessage = "Product code cannot exceed 16 characters")]
        public string ProductCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Product name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string ProductName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Original price cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Original price must be a valid decimal")]
        public decimal OriginalPrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Discount price must be a valid decimal")]
        public decimal? DiscountPrice { get; set; }
        [Required(ErrorMessage = "Initial quantity cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Initial quantity must be a valid integer")]
        public int InitialQuantity { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Warranty period cannot be empty")]
        [Range(0, int.MaxValue, ErrorMessage = "Warranty period must be a valid integer")]
        public int WarrantyPeriod { get; set; }
        [Required(ErrorMessage = "Category ID cannot be empty")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Brand ID cannot be empty")]
        public int BrandId { get; set; }
        public List<CreateProductSpecificationDTO> Specifications { get; set; } = new();
        public List<CreateProductImageDTO> Images { get; set; } = new();
    }
}