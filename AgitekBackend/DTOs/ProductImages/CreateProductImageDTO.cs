using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.ProductImages
{
    public class CreateProductImageDTO
    {
        [Required(ErrorMessage = "Product ID cannot be empty")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Image URL cannot be empty")]
        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string ImageUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "Sort order cannot be empty")]
        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a valid integer")]
        public int SortOrder { get; set; }
    }
}


