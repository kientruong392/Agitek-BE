using AgitekBackend.Common;
using AgitekBackend.DTOs.Orders;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync(int userId, bool assignedToDelivery = false);
        Task<OrderDTO> GetByIdAsync(int id);
        Task<OrderDTO> CreateAsync(CreateOrderDTO dto, int currentUserId);
        Task<OrderDTO> UpdateAsync(int id, UpdateOrderDTO dto);
        Task<OrderDTO> CancelOrderAsync(int id);
        Task<OrderDTO> ConfirmOrderAsync(int id, ConfirmOrderDTO dto);
        Task<OrderDTO> ShipOrderAsync(int id, ShipOrderDTO dto);
        Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus status);
    }
}