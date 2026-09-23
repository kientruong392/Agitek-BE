using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace AgitekBackend.DTOs.Categories
{
    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Category code can not be empty")]
        [Length(3, 6, ErrorMessage = "Category code must be between 3 and 6 characters")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Category name can not be empty")]
        [MaxLength(50, ErrorMessage = "Category name can not exceed 50 characters")]
        public string CategoryName { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }   
}


