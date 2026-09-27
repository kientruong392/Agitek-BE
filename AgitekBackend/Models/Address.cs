using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgitekBackend.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required, MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, MaxLength(255)]
        public string AddressLine { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public Location? Location { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}