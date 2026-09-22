using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Users
{
    public class UpdateMyAccountDTO
    {
        [Length(6, 20, ErrorMessage = "Username must be greater than 6 characters and less than 20 characters.")]
        public string? Username { get; set; }
        
        [MaxLength(320)]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        public string? Email { get; set; }
        
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Old password must be greater than 6 characters.")]
        public string? OldPassword { get; set; }
        
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be greater than 6 characters.")]
        public string? NewPassword { get; set; }
    }
}
