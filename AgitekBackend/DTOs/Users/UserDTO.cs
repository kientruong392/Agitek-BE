using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Users
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RoleName Role { get; set; }
        public int? GrantedByUserId { get; set; }
        public DateTime? RoleGrantedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}


