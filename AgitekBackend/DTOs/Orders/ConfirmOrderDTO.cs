using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Orders
{
    public class ConfirmOrderDTO
    {
        [Required(ErrorMessage = "Order status cannot be empty")]
        public OrderStatus Status { get; set; }
        public int? DeliveredByUserId { get; set; }
    }
}
