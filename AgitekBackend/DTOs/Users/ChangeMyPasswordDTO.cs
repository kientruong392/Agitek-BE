using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Users
{
    public class ChangeMyPasswordDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
