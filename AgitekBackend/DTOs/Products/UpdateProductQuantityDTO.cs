using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Products
{
    public class UpdateProductQuantityDTO
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a valid integer")]
        public int StockQuantity { get; set; }
    }
}
