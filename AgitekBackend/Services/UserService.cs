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

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.GrantedByUser)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<User?> GetRawByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail ||
                    u.Email == usernameOrEmail);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserDTO> CreateAsync(CreateUserDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = AuthUtils.HashPassword(dto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> UpdateAsync(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateVerifyStatusAsync(int id, bool isVerified)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            user.IsVerified = isVerified;
            user.UpdatedAt  = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            user.PasswordHash = AuthUtils.HashPassword(newPassword);
            user.UpdatedAt    = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task AddProviderAsync(int id, Provider provider)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundException($"User with id {id} not found.");
            
            if (user.Providers == null)
            {
                user.Providers = new List<Provider>();
            }

            if (!user.Providers.Contains(provider))
            {
                user.Providers.Add(provider);
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
