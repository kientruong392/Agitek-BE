using AgitekBackend.DTOs.Orders;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync();
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<OrderDTO> CreateAsync(CreateOrderDTO dto);
        Task<OrderDTO?> UpdateAsync(int id, UpdateOrderDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


