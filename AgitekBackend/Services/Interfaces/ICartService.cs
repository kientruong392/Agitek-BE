using AgitekBackend.DTOs.Carts;

namespace AgitekBackend.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(string cartKey, string productId, int quantity);
        Task<IEnumerable<CartItemDTO>> GetCartAsync(string cartKey);
        Task RemoveFromCartAsync(string cartKey, string productId);
        Task MergeCartAsync(string guestCartKey, string userCartKey);
    }
}
