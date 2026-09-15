using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        [MaxLength(1000)]
        public string? Comment { get; set; }
        public int? ReplyUserId { get; set; }
        [MaxLength(1000)]
        public string? ReplyComment { get; set; }
        public DateTime? ReplyCreatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
        public User? ReplyUser { get; set; }
        public ICollection<Media> Media { get; set; } = new List<Media>();
    }
}