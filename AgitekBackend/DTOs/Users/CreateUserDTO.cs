using AgitekBackend.Common;
using AgitekBackend.DTOs.Profiles;

namespace AgitekBackend.DTOs.Users
{
    public class CreateUserDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public RoleName Role { get; set; } = RoleName.Customer;
        public CreateProfileDTO Profile { get; set; } = new CreateProfileDTO();
    }
}


