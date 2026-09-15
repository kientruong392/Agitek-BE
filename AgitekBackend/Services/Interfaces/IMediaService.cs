using AgitekBackend.DTOs.Medias;

namespace AgitekBackend.Services.Interfaces
{
    public interface IMediaService
    {
        Task<IEnumerable<MediaDTO>> GetAllAsync();
        Task<MediaDTO?> GetByIdAsync(int id);
        Task<MediaDTO> CreateAsync(CreateMediaDTO dto);
        Task<MediaDTO?> UpdateAsync(int id, UpdateMediaDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


