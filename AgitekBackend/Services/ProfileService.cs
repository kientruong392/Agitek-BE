using AgitekBackend.Data;
using AgitekBackend.DTOs.Profiles;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProfileModel = AgitekBackend.Models.Profile;

namespace AgitekBackend.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProfileService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProfileDTO>> GetAllAsync()
        {
            var profiles = await _context.Profiles.ToListAsync();
            return _mapper.Map<IEnumerable<ProfileDTO>>(profiles);
        }

        public async Task<ProfileDTO?> GetByUserIdAsync(int userId)
        {
            var profile = await _context.Profiles.FindAsync(userId);
            if (profile == null) throw new NotFoundException($"Profile with userId {userId} not found.");
            return _mapper.Map<ProfileDTO>(profile);
        }
        public async Task<ProfileDTO?> UpdateAsync(int userId, UpdateProfileDTO dto)
        {
            var profile = await _context.Profiles.FindAsync(userId);
            if (profile == null) throw new NotFoundException($"Profile with userId {userId} not found.");

            _mapper.Map(dto, profile);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProfileDTO>(profile);
        }
    }
}



