using AgitekBackend.DTOs.Products;

namespace AgitekBackend.DTOs.Brands
{
    public class BrandDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public List<ProductDTO> Products { get; set; } = new();
    }
}


