using AgitekBackend.Common;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;

namespace AgitekBackend.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDTO>> GetAllAsync();
        Task<PaymentDTO> CreateAsync(CreatePaymentDTO dto);
        Task<IEnumerable<PaymentDTO>> GetByOrderIdAsync(int orderId);
        Task<PaymentDTO> GetByIdAsync(int id);
        Task<PaymentDTO?> GetPendingPaymentAsync(int orderId);
        Task<PublicPaymentDTO> GetPublicByOrderAsync(int orderId, string trackingNumber);
        Task UpdateStatusAsync(int id, PaymentStatus status);
    }
}
