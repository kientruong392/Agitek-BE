using AgitekBackend.DTOs.Profiles;

namespace AgitekBackend.Services.Interfaces
{
    public interface IProfileService
    {
        Task<IEnumerable<ProfileDTO>> GetAllAsync();
        Task<ProfileDTO?> GetByUserIdAsync(int userId);
        Task<ProfileDTO> CreateAsync(CreateProfileDTO dto);
        Task<ProfileDTO?> UpdateAsync(int userId, UpdateProfileDTO dto);
        Task<bool> DeleteAsync(int userId);
    }
}


