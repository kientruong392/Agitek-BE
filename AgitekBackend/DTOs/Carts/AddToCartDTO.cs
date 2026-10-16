using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Carts
{
    public class AddToCartDTO
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [RegularExpression(@"^[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}$")]
        public string? GuestSessionId { get; set; }
    }
}
