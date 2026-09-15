namespace AgitekBackend.DTOs.ProductImages
{
    public class UpdateProductImageDTO
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}


