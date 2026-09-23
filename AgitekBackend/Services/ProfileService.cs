using AgitekBackend.Data;
using AgitekBackend.DTOs.Profiles;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ProfileService(AppDbContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<IEnumerable<ProfileDTO>> GetAllAsync()
        {
            var profiles = await _context.Profiles.ToListAsync();
            return _mapper.Map<IEnumerable<ProfileDTO>>(profiles);
        }

        public async Task<ProfileDTO?> GetByUserIdAsync(int userId)
        {
            var profile = await _context.Profiles.FindAsync(userId);
            if (profile == null) throw new NotFoundException($"Profile with userId {userId} not found");
            return _mapper.Map<ProfileDTO>(profile);
        }
        public async Task<ProfileDTO?> UpdateAsync(int userId, UpdateProfileDTO dto)
        {
            var profile = await _context.Profiles.FindAsync(userId);
            if (profile == null) throw new NotFoundException($"Profile with userId {userId} not found");
            _mapper.Map(dto, profile);
            if (string.IsNullOrEmpty(profile.FullName))
            {
                var user = await _userService.GetByIdAsync(profile.UserId);
                profile.FullName = user != null ? user.Username : "Deleted_User";
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<ProfileDTO>(profile);
        }
    }
}



