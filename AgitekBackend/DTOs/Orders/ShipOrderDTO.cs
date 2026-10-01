using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Medias;

namespace AgitekBackend.DTOs.Orders
{
    public class ShipOrderDTO
    {
        [Required(ErrorMessage = "Order status cannot be empty")]
        public OrderStatus Status { get; set; }
        public List<CreateMediaDTO> Medias { get; set; } = new();
    }
}