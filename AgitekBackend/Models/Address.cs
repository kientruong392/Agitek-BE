using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Street { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Ward { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public User? User { get; set; }
    }
}