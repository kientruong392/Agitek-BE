using AgitekBackend.DTOs.Locations;

namespace AgitekBackend.DTOs.Addresses
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public LocationDTO? Location { get; set; }
        public bool IsDefault { get; set; }
    }
}
