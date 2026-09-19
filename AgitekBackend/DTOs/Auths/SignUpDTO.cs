using AgitekBackend.DTOs.Profiles;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Auths
{
    public class SignUpDTO
    {
        [Required, Length(6, 20, ErrorMessage = "Username must be greater than 6 characters and less than 20 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be greater than 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password cannot be empty")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public CreateProfileDTO Profile { get; set; } = new CreateProfileDTO();
    }
}
