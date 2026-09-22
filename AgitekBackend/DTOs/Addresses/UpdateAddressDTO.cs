namespace AgitekBackend.DTOs.Addresses
{
    public class UpdateAddressDTO
    {
        public int? UserId { get; set; }
        public string? ReceiverName { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string? Street { get; set; } 
        public string? Ward { get; set; } 
        public string? District { get; set; } 
        public string? City { get; set; } 
        public bool? IsDefault { get; set; }
    }
}


