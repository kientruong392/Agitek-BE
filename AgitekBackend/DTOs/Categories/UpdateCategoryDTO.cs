namespace AgitekBackend.DTOs.Categories
{
    public class UpdateCategoryDTO
    {
        public string? Code { get; set; } 
        public string? CategoryName { get; set; } 
        public string? Slug { get; set; } 
        public int? ParentCategoryId { get; set; }
    }
}


