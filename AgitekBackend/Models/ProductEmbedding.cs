using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class ProductEmbedding
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(64)]
        public string SourceHash { get; set; } = string.Empty;

        [Required]
        public string VectorJson { get; set; } = string.Empty;

        public int Dimension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}
