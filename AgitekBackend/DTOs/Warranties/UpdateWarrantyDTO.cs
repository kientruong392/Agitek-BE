using System.ComponentModel.DataAnnotations;
using AgitekBackend.DTOs.Medias;

namespace AgitekBackend.DTOs.Warranties
{
    public class UpdateWarrantyDTO
    {
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        [MaxLength(1000, ErrorMessage = "Product condition cannot exceed 1000 characters")]
        public string? ProductCondition { get; set; }
        public List<CreateMediaDTO>? Medias { get; set; }
    }
}