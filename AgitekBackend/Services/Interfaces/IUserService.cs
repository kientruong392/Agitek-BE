using AgitekBackend.Common;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;

namespace AgitekBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<IEnumerable<UserDTO>> GetAllCustomersAsync();
        Task<IEnumerable<UserDTO>> GetAllStaffAsync();
        Task<UserDTO> GetByIdAsync(int id);
        Task<UserDTO> CreateAsync(CreateUserDTO dto, int? currentUserId = null);
        Task<UserDTO> CreateStaffAsync(CreateStaffDTO dto, int currentUserId);
        Task<UserDTO> UpdateRoleAsync(int id, UpdateUserRoleDTO dto, int currentUserId, RoleName currentRole);
        Task<UserDTO> UpdateStatusAsync(int id, UpdateUserStatusDTO dto);
        Task<UserDTO> UpdateAccountAsync(int id, UpdateMyAccountDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<UserDTO> GetByEmailAsync(string email);
        Task UpdateVerifyStatusAsync(int id, bool isVerified);
        Task ChangePasswordAsync(int id, string newPassword);
        Task AddProviderAsync(int id, Provider provider);
        Task<UserDTO?> CheckEmailExists(string email);
    }
}
