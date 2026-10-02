using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Warranties
{
    public class ApproveWarrantyDTO
    {
        [Required(ErrorMessage = "Warranty status cannot be empty")]
        public WarrantyStatus Status { get; set; }
        [Required(ErrorMessage = "Response cannot be empty")]
        [MaxLength(500, ErrorMessage = "Response cannot exceed 500 characters")]
        public string Response { get; set; } = string.Empty;
    }
}