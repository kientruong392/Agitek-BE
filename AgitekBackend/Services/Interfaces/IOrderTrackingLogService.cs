using AgitekBackend.DTOs.OrderTrackingLogs;

namespace AgitekBackend.Services.Interfaces
{
    public interface IOrderTrackingLogService
    {
        Task<IEnumerable<OrderTrackingLogDTO>> GetAllAsync();
        Task<OrderTrackingLogDTO?> GetByIdAsync(int id);
        Task<OrderTrackingLogDTO> CreateAsync(CreateOrderTrackingLogDTO dto);
        Task<OrderTrackingLogDTO?> UpdateAsync(int id, UpdateOrderTrackingLogDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


