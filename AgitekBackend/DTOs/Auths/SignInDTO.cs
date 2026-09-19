using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Auths
{
    public class SignInDTO
    {
        [Required(ErrorMessage = "Username or Email cannot be empty")]
        public string UsernameOrEmail { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password cannot be empty")]
        public string Password { get; set; } = string.Empty;
    }
}
