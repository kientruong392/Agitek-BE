namespace AgitekBackend.DTOs.ProductImages
{
    public class CreateProductImageDTO
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int SortOrder { get; set; }
    }
}


