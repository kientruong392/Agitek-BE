using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.OrderDetails
{
    public class CreateOrderDetailDTO
    {
        [Required(ErrorMessage = "Product Id cannot be empty")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Quantity cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Unit price cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a valid decimal")]
        public decimal UnitPrice { get; set; }
        [Required(ErrorMessage = "Total price cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Total price must be a valid decimal")]
        public decimal TotalPrice { get; set; }
    }
}
