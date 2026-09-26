using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.ProductSpecifications
{
    public class UpdateProductSpecificationDTO
    {
        public int? ProductId { get; set; }
        [MaxLength(50, ErrorMessage = "Specification name cannot exceed 50 characters")]
        public string? SpecName { get; set; }
        [MaxLength(200, ErrorMessage = "Specification value cannot exceed 200 characters")]
        public string? SpecValue { get; set; }
    }
}