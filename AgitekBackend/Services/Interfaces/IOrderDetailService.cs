using AgitekBackend.DTOs.OrderDetails;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<IEnumerable<OrderDetailDTO>> GetAllAsync();
        Task<OrderDetailDTO?> GetByIdAsync(int orderId, int productId);
        Task<OrderDetailDTO> CreateAsync(CreateOrderDetailDTO dto);
        Task<OrderDetailDTO?> UpdateAsync(int orderId, int productId, UpdateOrderDetailDTO dto);
        Task<bool> DeleteAsync(int orderId, int productId);
    }
}


