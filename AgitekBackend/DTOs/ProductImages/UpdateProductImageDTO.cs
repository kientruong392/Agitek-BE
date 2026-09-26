using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.ProductImages
{
    public class UpdateProductImageDTO
    {
        public int? ProductId { get; set; }
        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a valid integer")]
        public int? SortOrder { get; set; }
    }
}


