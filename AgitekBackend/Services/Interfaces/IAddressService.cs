using AgitekBackend.DTOs.Addresses;

namespace AgitekBackend.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDTO>> GetByUserIdAsync(int userId);
        Task<AddressDTO?> GetByIdAsync(int id);
        Task<AddressDTO> CreateAsync(CreateAddressDTO dto, int currentUserId);
        Task<AddressDTO?> UpdateAsync(int id, UpdateAddressDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
