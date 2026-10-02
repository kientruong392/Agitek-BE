using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Reviews
{
    public class ReplyReviewDTO
    {
        [Required(ErrorMessage = "ReplyComment cannot be empty")]
        public string ReplyComment { get; set; } = string.Empty;
    }
}