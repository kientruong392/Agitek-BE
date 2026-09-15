using AgitekBackend.DTOs.Addresses;

namespace AgitekBackend.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDTO>> GetAllAsync();
        Task<AddressDTO?> GetByIdAsync(int id);
        Task<AddressDTO> CreateAsync(CreateAddressDTO dto);
        Task<AddressDTO?> UpdateAsync(int id, UpdateAddressDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


