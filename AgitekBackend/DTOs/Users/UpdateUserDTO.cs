using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Users
{
    public class UpdateUserDTO
    {
        [Required, Length(6, 20, ErrorMessage = "Username must be greater than 6 characters and less than 20 characters.")]
        public string? Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty.")]
        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        public string? Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password cannot be empty.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be greater than 6 characters.")]
        public string? Password { get; set; } = string.Empty;
        public RoleName? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}


