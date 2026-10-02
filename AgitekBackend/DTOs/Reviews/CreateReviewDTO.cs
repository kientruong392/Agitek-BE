using System.ComponentModel.DataAnnotations;
using AgitekBackend.DTOs.Medias;

namespace AgitekBackend.DTOs.Reviews
{
    public class CreateReviewDTO
    {
        [Required(ErrorMessage = "OrderId cannot be empty")]
        public int OrderId { get; set; }
        [Required(ErrorMessage = "ProductId cannot be empty")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Rating cannot be empty")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<CreateMediaDTO> Medias { get; set; } = new();
    }
}


