using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.OrderDetails
{
    public class CreateOrderDetailDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be greater than 0")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal TotalPrice { get; set; }
    }
}
