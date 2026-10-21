using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Refunds
{
    public class ApproveRefundDTO
    {
        [Required(ErrorMessage = "Refund status cannot be empty")]
        public RefundStatus Status { get; set; }
        [Required(ErrorMessage = "Refund response cannot be empty")]
        [MaxLength(500, ErrorMessage = "Refund response cannot exceed 500 characters")]
        public string Response { get; set; } = string.Empty;
    }
}
