using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Profiles
{
    public class CreateProfileDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public GenderType? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}


