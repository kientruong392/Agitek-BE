using AgitekBackend.Common;
using AgitekBackend.DTOs.Products;

namespace AgitekBackend.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync(CurrentUser? currentUser);
        Task<ProductDTO> GetByIdAsync(int id);
        Task<ProductDTO> CreateAsync(CreateProductDTO dto, int currentUserId);
        Task<ProductDTO> UpdateAsync(int id, UpdateProductDTO dto);
        Task<bool> ChangeStatusAsync(int id, ProductStatus newStatus);
        Task<bool> UpdateQuantityAsync(int id, int stockQuantity);
        Task<bool> UpdateInitialQuantityAsync(int id, int initialQuantity);
        Task<bool> UpdateStockQuantityAsync(int id, int stockQuantity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}
