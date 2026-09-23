using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Brands
{
    public class CreateBrandDTO
    {
        [Required(ErrorMessage = "Brand code can not be empty")]
        [Length(3, 6, ErrorMessage = "Brand code must be between 3 and 6 characters")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Brand name can not be empty")]
        [MaxLength(50, ErrorMessage = "Brand name can not exceed 50 characters")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Brand logo URL can not exceed 500 characters")]
        public string? LogoUrl { get; set; }
    }
}


