using AgitekBackend.DTOs.Reviews;

namespace AgitekBackend.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTO>> GetAllAsync(int productId);
        Task<ReviewDTO> GetByIdAsync(int id);
        Task<ReviewDTO> CreateAsync(CreateReviewDTO dto, int currentUserId);
        Task<ReviewDTO> UpdateAsync(int id, UpdateReviewDTO dto);
        Task<ReviewDTO> ReplyAsync(int id, int currentUserId, ReplyReviewDTO dto);
    }
}


