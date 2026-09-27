using AgitekBackend.DTOs.Locations;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Addresses
{
    public class CreateAddressDTO
    {
        [Required(ErrorMessage = "Receiver Name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Receiver Name cannot exceed 100 characters")]
        public string ReceiverName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone Number cannot be empty")]
        [MaxLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Address Line cannot be empty")]
        [MaxLength(255, ErrorMessage = "Address Line cannot exceed 255 characters")]
        public string AddressLine { get; set; } = string.Empty;
        [Required(ErrorMessage = "Location cannot be empty")]
        public int LocationId { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}


