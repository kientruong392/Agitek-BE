using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class ProductSpecification
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public string SpecName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string SpecValue { get; set; } = string.Empty;

        public Product? Product { get; set; }
    }
}