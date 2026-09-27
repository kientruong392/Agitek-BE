using AgitekBackend.DTOs.Locations;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Addresses
{
    public class UpdateAddressDTO
    {
        [MaxLength(100, ErrorMessage = "Receiver Name cannot exceed 100 characters")]
        public string? ReceiverName { get; set; }
        [MaxLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
        [MaxLength(255, ErrorMessage = "Address Line cannot exceed 255 characters")]
        public string? AddressLine { get; set; }
        public int? LocationId { get; set; }
        public bool? IsDefault { get; set; }
    }
}


