using AgitekBackend.DTOs.AuditLogs;

namespace AgitekBackend.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogPageDTO> GetPageAsync(AuditLogQueryDTO query);
        Task<AuditLogDTO> GetByIdAsync(int id);
        Task RecordStockReceiptAsync(int productId, int quantity, int? actorId, DateTime timestamp);
        Task RecordActionAsync(string entityName, string action);
        Task<List<StockReceiptDTO>> GetStockReceiptsAsync(DateTime from, DateTime to);
    }
}
