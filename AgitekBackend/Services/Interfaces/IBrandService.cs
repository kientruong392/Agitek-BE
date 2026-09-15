using AgitekBackend.DTOs.Brands;

namespace AgitekBackend.Services.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDTO>> GetAllAsync();
        Task<BrandDTO?> GetByIdAsync(int id);
        Task<BrandDTO> CreateAsync(CreateBrandDTO dto);
        Task<BrandDTO?> UpdateAsync(int id, UpdateBrandDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


