using AgitekBackend.DTOs.Products;

namespace AgitekBackend.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<ProductDTO> CreateAsync(CreateProductDTO dto);
        Task<ProductDTO?> UpdateAsync(int id, UpdateProductDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


