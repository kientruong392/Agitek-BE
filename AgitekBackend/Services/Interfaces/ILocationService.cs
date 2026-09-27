using AgitekBackend.DTOs.Locations;

namespace AgitekBackend.Services.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDTO>> GetAllAsync();
        Task<LocationDTO?> GetByIdAsync(int id);
        Task<LocationDTO> CreateAsync(CreateLocationDTO dto);
        Task<LocationDTO?> UpdateAsync(int id, UpdateLocationDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
