using AgitekBackend.DTOs.AuditLogs;

namespace AgitekBackend.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDTO>> GetAllAsync();
        Task<AuditLogDTO?> GetByIdAsync(int id);
        Task<AuditLogDTO> CreateAsync(CreateAuditLogDTO dto);
        Task<AuditLogDTO?> UpdateAsync(int id, UpdateAuditLogDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


