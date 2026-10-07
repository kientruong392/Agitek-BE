using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Profiles
{
    public class CreateProfileDTO
    {
        [MaxLength(100, ErrorMessage = "Full name can only contain up to 100 characters")]
        public string? FullName { get; set; } = string.Empty;
        [RegularExpression(@"^(?:\+84|0)(3[2-9]|5[2689]|7[06-9]|8[1-9]|9[0-9])[0-9]{7}$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
        public GenderType? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}