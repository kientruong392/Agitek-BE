using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Auths
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
