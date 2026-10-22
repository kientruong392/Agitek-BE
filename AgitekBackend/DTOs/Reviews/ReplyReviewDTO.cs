using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Reviews
{
    public class ReplyReviewDTO
    {
        [Required(ErrorMessage = "ReplyComment cannot be empty")]
        [MaxLength(1000, ErrorMessage = "ReplyComment cannot exceed 1000 characters")]
        public string ReplyComment { get; set; } = string.Empty;
    }
}
