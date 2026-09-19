using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Profile
    {
        [Key]
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
        [MaxLength(10)]
        public GenderType Gender { get; set; } = GenderType.Other;
        public DateTime? DateOfBirth { get; set; }
        public User? User { get; set; }
    }
}