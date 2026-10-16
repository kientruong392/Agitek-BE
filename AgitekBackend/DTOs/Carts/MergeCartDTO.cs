using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Carts
{
    public class MergeCartDTO
    {
        [Required]
        [RegularExpression(@"^[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}$")]
        public string GuestSessionId { get; set; } = string.Empty;
    }
}
