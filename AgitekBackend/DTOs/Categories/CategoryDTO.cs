namespace AgitekBackend.DTOs.Categories
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }
}


