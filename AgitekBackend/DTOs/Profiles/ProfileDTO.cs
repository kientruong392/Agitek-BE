using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Profiles
{
    public class ProfileDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public GenderType? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}


