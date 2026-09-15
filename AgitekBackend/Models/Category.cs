using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, Length(3, 6)]
        public string Code { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;
        [Required, MaxLength(150)]
        public string Slug { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}