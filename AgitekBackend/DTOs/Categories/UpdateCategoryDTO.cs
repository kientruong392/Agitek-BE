using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Categories
{
    public class UpdateCategoryDTO
    {
        [Length(3, 6, ErrorMessage = "Category code must be between 3 and 6 characters")]
        public string? Code { get; set; }
        [MaxLength(50, ErrorMessage = "Category name can not exceed 50 characters")]
        public string? CategoryName { get; set; }
    }
}


