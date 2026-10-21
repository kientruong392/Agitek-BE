using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Locations;

namespace AgitekBackend.DTOs.Orders
{
    public class ConfirmOrderDTO
    {
        [Required(ErrorMessage = "Order status cannot be empty")]
        public OrderStatus Status { get; set; }
        public int? DeliveredByUserId { get; set; }

        [Range(1, int.MaxValue)]
        public int? StartingWarehouseId { get; set; }

        [MaxLength(255)]
        public string? StartingWarehouseDescription { get; set; }

        public CreateDeliveryLocationDTO? CreateLocation { get; set; }
    }
}
