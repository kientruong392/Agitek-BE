using AgitekBackend.Common;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.OrderTrackingLogs;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync(int? userId = null, bool assignedToDelivery = false);
        Task<OrderDTO> GetByIdAsync(int id);
        Task<OrderDTO> CreateAsync(CreateOrderDTO dto, int currentUserId);
        Task<OrderDTO> CreateGuestOrderAsync(CreateGuestOrderDTO dto);
        Task<OrderDTO> UpdateAsync(int id, UpdateOrderDTO dto);
        Task<OrderDTO> CancelOrderAsync(int id);
        Task<OrderDTO> CompleteOrderAsync(int id);
        Task<OrderDTO> ConfirmOrderAsync(int id, ConfirmOrderDTO dto);
        Task<OrderDTO> StartTransitAsync(int id, CurrentUser currentUser);
        Task<OrderDTO> ShipOrderAsync(int id, ShipOrderDTO dto);
        Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<IEnumerable<object>> GetDeliveryUsersAsync();
        Task<IEnumerable<OrderTrackingLogDTO>> GetTrackingLogsAsync(int orderId);
        Task<IEnumerable<OrderTrackingLogDTO>> AddTrackingLogAsync(int orderId, CreateOrderTrackingLogDTO dto, CurrentUser currentUser);
    }
}
