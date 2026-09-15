using AgitekBackend.DTOs.ProductImages;

namespace AgitekBackend.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDTO>> GetAllAsync();
        Task<ProductImageDTO?> GetByIdAsync(int id);
        Task<ProductImageDTO> CreateAsync(CreateProductImageDTO dto);
        Task<ProductImageDTO?> UpdateAsync(int id, UpdateProductImageDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


