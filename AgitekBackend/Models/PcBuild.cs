using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class PcBuild
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
