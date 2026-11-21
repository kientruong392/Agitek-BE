using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Common.Utils;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.GrantedByUser)
                .Include(u => u.Profile)
                .ToListAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.GrantedByUser)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User not found");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> GetMeAsync(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException("User not found");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task ChangeMyPasswordAsync(int id, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException("User not found");
            if (user.PasswordHash == null || !AuthUtils.VerifyPassword(oldPassword, user.PasswordHash))
                throw new BadRequestException("Incorrect old password");

            user.PasswordHash = AuthUtils.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserDTO>> GetAllCustomersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.Role == RoleName.Customer || u.Role == RoleName.Guest)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<IEnumerable<UserDTO>> GetAllStaffAsync()
        {
            var users = await _context.Users
                .Include(u => u.GrantedByUser)
                .Include(u => u.Profile)
                .Where(u => u.Role != RoleName.Customer && u.Role != RoleName.Guest)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail ||
                    u.Email == usernameOrEmail);
            return user;
        }

        public async Task<UserDTO> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.GrantedByUser)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new NotFoundException($"User not found");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateAsync(CreateUserDTO dto, int? currentUserId = null)
        {
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = string.IsNullOrEmpty(dto.Password) ? null : AuthUtils.HashPassword(dto.Password);
            user.Profile.FullName = !string.IsNullOrEmpty(user.Profile.FullName) ? user.Profile.FullName : user.Username ?? "Undefined";
            if (currentUserId.HasValue)
            {
                user.GrantedByUserId = currentUserId.Value;
            }
            if (user.Role != RoleName.Customer && user.Role != RoleName.Guest)
            {
                user.IsVerified = true;
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateStaffAsync(CreateStaffDTO dto, int currentUserId)
        {
            if (dto.Role != RoleName.Manager && dto.Role != RoleName.Staff && dto.Role != RoleName.Delivery)
                throw new BadRequestException("Invalid role assignment");

            var usernameExists = await GetByUsernameOrEmailAsync(dto.Username);
            var emailExists = await GetByUsernameOrEmailAsync(dto.Email);
            if (usernameExists != null || emailExists != null)
                throw new BadRequestException("Username or Email already exists");

            var createUserDto = _mapper.Map<CreateUserDTO>(dto);

            var userDto = await CreateAsync(createUserDto, currentUserId);
            await AddProviderAsync(userDto.Id, Provider.Credentials);
            return userDto;
        }

        public async Task<UserDTO> UpdateRoleAsync(int id, UpdateUserRoleDTO dto, int currentUserId, RoleName currentRole)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User not found");

            // Business logic checks
            if (user.Role == RoleName.Customer && dto.Role != RoleName.Customer)
            {
                throw new BadRequestException("Customer role cannot be changed to another role");
            }
            if (user.Role == RoleName.Guest && dto.Role != RoleName.Guest && dto.Role != RoleName.Customer)
            {
                throw new BadRequestException("Guest role can only be upgraded to Customer");
            }
            if (currentRole == RoleName.Admin && (dto.Role == RoleName.Admin || dto.Role == RoleName.SuperAdmin))
            {
                throw new ForbiddenException("You do not have permission to grant this role");
            }

            user.Role = dto.Role;
            user.GrantedByUserId = currentUserId;
            user.RoleGrantedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            // Reload with relations to map correctly
            return await GetByIdAsync(id);
        }

        public async Task<UserDTO> UpdateStatusAsync(int id, UpdateUserStatusDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User not found");

            if (user.Role == RoleName.SuperAdmin && !dto.IsActive)
            {
                throw new BadRequestException("Cannot ban a Super Admin");
            }

            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateAccountAsync(int id, UpdateMyAccountDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User not found");

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
                if (existingUser != null)
                {
                    throw new BadRequestException("Username is already taken");
                }
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingEmail != null)
                {
                    throw new BadRequestException("Email is already taken");
                }
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (string.IsNullOrEmpty(dto.OldPassword))
                {
                    throw new BadRequestException("Old password is required to set a new password");
                }
                
                if (user.PasswordHash == null || !AuthUtils.VerifyPassword(dto.OldPassword, user.PasswordHash))
                {
                    throw new BadRequestException("Incorrect old password");
                }
                
                user.PasswordHash = AuthUtils.HashPassword(dto.NewPassword);
            }

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User not found");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateVerifyStatusAsync(int id, bool isVerified)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User not found");
            user.IsVerified = isVerified;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User not found");
            user.PasswordHash = AuthUtils.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task AddProviderAsync(int id, Provider provider)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User not found");
            if (!user.Providers.Contains(provider))
            {
                user.Providers.Add(provider);
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserDTO?> CheckEmailExists(string email)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == email);
            return user == null ? null : _mapper.Map<UserDTO>(user);
        }
    }
}
