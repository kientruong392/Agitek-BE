using System.ComponentModel.DataAnnotations;
using AgitekBackend.Common;

namespace AgitekBackend.DTOs.Refunds
{
    public class ApproveRefundDTO
    {
        [Required(ErrorMessage = "Refund status cannot be empty")]
        public RefundStatus Status { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}