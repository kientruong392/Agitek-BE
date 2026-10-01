using AgitekBackend.DTOs.Medias;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Refunds
{
    public class CreateRefundDTO
    {
        [Required]
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Quantity cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        public ICollection<CreateMediaDTO> Medias { get; set; } = new List<CreateMediaDTO>();
    }
}
