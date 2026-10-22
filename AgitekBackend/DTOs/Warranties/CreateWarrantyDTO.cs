using System.ComponentModel.DataAnnotations;
using AgitekBackend.DTOs.Medias;

namespace AgitekBackend.DTOs.Warranties
{
    public class CreateWarrantyDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Quantity cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Full name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address cannot be empty")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "Product condition cannot be empty")]
        [MaxLength(1000, ErrorMessage = "Product condition cannot exceed 1000 characters")]
        public string ProductCondition { get; set; } = string.Empty;
        public List<CreateMediaDTO> Medias { get; set; } = new();
    }
}
