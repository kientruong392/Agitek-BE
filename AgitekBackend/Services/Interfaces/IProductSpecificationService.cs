using AgitekBackend.DTOs.ProductSpecifications;

namespace AgitekBackend.Services.Interfaces
{
    public interface IProductSpecificationService
    {
        Task<IEnumerable<ProductSpecificationDTO>> GetAllAsync();
        Task<ProductSpecificationDTO?> GetByIdAsync(int id);
        Task<ProductSpecificationDTO> CreateAsync(CreateProductSpecificationDTO dto);
        Task<ProductSpecificationDTO?> UpdateAsync(int id, UpdateProductSpecificationDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


