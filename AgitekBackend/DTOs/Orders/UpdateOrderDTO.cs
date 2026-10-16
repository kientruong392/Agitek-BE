using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Orders
{
    public class UpdateOrderDTO
    {
        public int? AddressId { get; set; }
        public ShippingType? ShippingType { get; set; }
        [MaxLength(255)]
        public string? Note { get; set; }
    }
}