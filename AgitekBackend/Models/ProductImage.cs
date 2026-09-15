using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int SortOrder { get; set; }

        public Product? Product { get; set; }
    }
}