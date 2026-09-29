using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Orders
{
    public class UpdateOrderDTO
    {
        public int? AddressId { get; set; }
        public string? Note { get; set; }
    }
}