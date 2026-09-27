using AgitekBackend.Common;
using AgitekBackend.DTOs.Locations;

namespace AgitekBackend.Services.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDTO>> GetAllAsync();
        Task<LocationDTO> GetByIdAsync(int id);
        Task<LocationDTO> GetByIdWithChildrensAsync(int id);
        Task<LocationDTO> CreateAsync(CreateLocationDTO dto, CurrentUser? currentUser);
        Task<LocationDTO> UpdateAsync(int id, UpdateLocationDTO dto, CurrentUser? currentUser);
        Task<bool> DeleteAsync(int id, CurrentUser? currentUser);
    }
}