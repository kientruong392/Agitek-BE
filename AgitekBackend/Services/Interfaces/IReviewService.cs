using AgitekBackend.DTOs.Reviews;

namespace AgitekBackend.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTO>> GetAllAsync();
        Task<ReviewDTO?> GetByIdAsync(int id);
        Task<ReviewDTO> CreateAsync(CreateReviewDTO dto);
        Task<ReviewDTO?> UpdateAsync(int id, UpdateReviewDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


