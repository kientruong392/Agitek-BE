using AgitekBackend.Common;
using System.ComponentModel.DataAnnotations;

namespace AgitekBackend.DTOs.Products
{
    public class UpdateProductStatusDTO
    {
        [Required(ErrorMessage = "Status is required")]
        public ProductStatus Status { get; set; }
    }
}
