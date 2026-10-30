using AgitekBackend.DTOs.Products;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.PcBuilds
{
    public class PcBuildDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required, MaxLength(8)]
        public List<int> ProductIds { get; set; } = new();
        public List<ProductDTO> Products { get; set; } = new();
    }
}
