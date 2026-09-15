namespace AgitekBackend.DTOs.Categories
{
    public class UpdateCategoryDTO
    {
        public string Code { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }
}


