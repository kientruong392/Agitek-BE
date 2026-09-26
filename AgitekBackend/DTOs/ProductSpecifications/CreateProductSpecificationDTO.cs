using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.ProductSpecifications
{
    public class CreateProductSpecificationDTO
    {
        [Required(ErrorMessage = "Product ID cannot be empty")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Specification name cannot be empty")]
        [MaxLength(50, ErrorMessage = "Specification name cannot exceed 50 characters")]
        public string SpecName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specification value cannot be empty")]
        [MaxLength(200, ErrorMessage = "Specification value cannot exceed 200 characters")]
        public string SpecValue { get; set; } = string.Empty;
    }
}