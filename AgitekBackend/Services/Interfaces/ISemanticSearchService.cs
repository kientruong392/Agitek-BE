using AgitekBackend.Common;
using AgitekBackend.DTOs.Products;

namespace AgitekBackend.Services.Interfaces
{
    public interface ISemanticSearchService
    {
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string query, int maxResults, CurrentUser? currentUser);
        Task SynchronizeProductAsync(int productId);
        Task SynchronizeAllProductsAsync(CancellationToken cancellationToken = default);
    }
}
