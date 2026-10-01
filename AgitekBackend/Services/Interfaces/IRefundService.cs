using AgitekBackend.DTOs.Refunds;

namespace AgitekBackend.Services.Interfaces
{
    public interface IRefundService
    {
        Task<IEnumerable<RefundDTO>> GetAllAsync();
        Task<RefundDTO> GetByIdAsync(int orderId, int productId);
        Task<RefundDTO> CreateAsync(CreateRefundDTO dto, int currentUserId);
        Task<RefundDTO> UpdateAsync(int orderId, int productId, int currentUserId, ApproveRefundDTO dto);
    }
}
