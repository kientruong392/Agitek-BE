using AgitekBackend.Common;
using AgitekBackend.DTOs.PcBuilds;
using AgitekBackend.DTOs.Products;

namespace AgitekBackend.Services.Interfaces
{
    public interface IPcBuildService
    {
        Task<IEnumerable<ProductDTO>> GetProductsBySpecsAsync(PcComponentType component, PcBuildDTO dto);
        Task<IEnumerable<ProductDTO>> GetCheckoutProductsAsync(int[] productIds);
        Task<IEnumerable<PcBuildDTO>> GetMyAsync(int userId);
        Task<PcBuildDTO> GetByIdAsync(int id, int userId);
        Task<PcBuildDTO> CreateAsync(SavePcBuildDTO dto, int userId);
        Task<PcBuildDTO> UpdateAsync(int id, SavePcBuildDTO dto, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
