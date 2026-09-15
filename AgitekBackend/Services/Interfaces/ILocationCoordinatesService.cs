using AgitekBackend.DTOs.LocationCoordinates;

namespace AgitekBackend.Services.Interfaces
{
    public interface ILocationCoordinatesService
    {
        Task<IEnumerable<LocationCoordinatesDTO>> GetAllAsync();
        Task<LocationCoordinatesDTO?> GetByIdAsync(int id);
        Task<LocationCoordinatesDTO> CreateAsync(CreateLocationCoordinatesDTO dto);
        Task<LocationCoordinatesDTO?> UpdateAsync(int id, UpdateLocationCoordinatesDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


