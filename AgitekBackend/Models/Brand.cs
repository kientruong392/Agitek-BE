using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}