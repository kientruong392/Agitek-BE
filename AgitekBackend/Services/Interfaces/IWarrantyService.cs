using AgitekBackend.DTOs.Warranties;

namespace AgitekBackend.Services.Interfaces
{
    public interface IWarrantyService
    {
        Task<IEnumerable<WarrantyDTO>> GetAllAsync();
        Task<WarrantyDTO?> GetByIdAsync(int id);
        Task<WarrantyDTO> CreateAsync(CreateWarrantyDTO dto);
        Task<WarrantyDTO?> UpdateAsync(int id, UpdateWarrantyDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


