namespace AgitekBackend.DTOs.Reviews
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int? ReplyUserId { get; set; }
        public string? ReplyComment { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


