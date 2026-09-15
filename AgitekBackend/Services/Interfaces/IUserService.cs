using AgitekBackend.DTOs.Users;

namespace AgitekBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);
        Task<UserDTO> CreateAsync(CreateUserDTO dto);
        Task<UserDTO?> UpdateAsync(int id, UpdateUserDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}


