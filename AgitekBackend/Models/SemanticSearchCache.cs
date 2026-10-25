using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class SemanticSearchCache
    {
        [Key, MaxLength(64)]
        public string QueryHash { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string NormalizedQuery { get; set; } = string.Empty;

        [Required]
        public string VectorJson { get; set; } = string.Empty;

        public int Dimension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
