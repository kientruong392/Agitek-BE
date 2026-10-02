using AgitekBackend.Common;
using AgitekBackend.DTOs.Warranties;

namespace AgitekBackend.Services.Interfaces
{
    public interface IWarrantyService
    {
        Task<IEnumerable<WarrantyDTO>> GetAllAsync(CurrentUser currentUser);
        Task<WarrantyDTO?> GetByIdAsync(int id);
        Task<WarrantyDTO> CreateAsync(CreateWarrantyDTO dto, int currentUserId);
        Task<WarrantyDTO?> UpdateAsync(int id, UpdateWarrantyDTO dto);
        Task<WarrantyDTO> ApproveAsync(int id, int currentUserId, ApproveWarrantyDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
