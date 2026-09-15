using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Users
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public RoleName Role { get; set; } = RoleName.Customer;
        public int? GrantedByUserId { get; set; }
        public DateTime? RoleGrantedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }
}


