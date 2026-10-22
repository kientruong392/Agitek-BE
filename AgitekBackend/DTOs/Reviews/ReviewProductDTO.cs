using AgitekBackend.DTOs.ProductImages;

namespace AgitekBackend.DTOs.Reviews
{
    public class ReviewProductDTO
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<ProductImageDTO> Images { get; set; } = new();
    }
}
