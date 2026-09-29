using AgitekBackend.Common;
using AgitekBackend.DTOs.Orders;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync(int userId);
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<OrderDTO> CreateAsync(CreateOrderDTO dto, int currentUserId);
        Task<OrderDTO?> UpdateAsync(int id, UpdateOrderDTO dto);
    }
}
