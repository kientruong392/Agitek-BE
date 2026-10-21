using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Locations;
using AgitekBackend.DTOs.OrderDetails;

namespace AgitekBackend.DTOs.Orders
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Address Id cannot be empty")]
        [Range(1, int.MaxValue)]
        public int AddressId { get; set; }

        [Range(1, int.MaxValue)]
        public int? VoucherId { get; set; }

        [EnumDataType(typeof(ShippingType))]
        public ShippingType ShippingType { get; set; } = ShippingType.Standard;

        [EnumDataType(typeof(PaymentMethod))]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        [MaxLength(255)]
        public string? Note { get; set; }

        public CreateDeliveryLocationDTO? CreateLocation { get; set; }

        [MaxLength(50)]
        public string? VoucherCode { get; set; }

        [Required(ErrorMessage = "Order details cannot be empty")]
        [MinLength(1, ErrorMessage = "Order must have at least one order detail")]
        public List<CreateOrderDetailDTO> OrderDetails { get; set; } = new();
    }
}
