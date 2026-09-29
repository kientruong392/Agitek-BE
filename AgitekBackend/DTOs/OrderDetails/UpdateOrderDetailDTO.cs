using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.OrderDetails
{
    public class UpdateOrderDetailDTO
    {
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a valid decimal")]
        public decimal? UnitPrice { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Total price must be a valid decimal")]
        public decimal? TotalPrice { get; set; }
    }
}
