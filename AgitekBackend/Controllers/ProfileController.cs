using AgitekBackend.DTOs.Profiles;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfileDTO>>> GetAll()
        {
            var profiles = await _profileService.GetAllAsync();
            return Ok(profiles);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ProfileDTO>> GetById(int userId)
        {
            var profile = await _profileService.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ProfileDTO>> Update(int userId, [FromBody] UpdateProfileDTO dto)
        {
            var updatedProfile = await _profileService.UpdateAsync(userId, dto);
            if (updatedProfile == null)
            {
                return NotFound();
            }
            return Ok(updatedProfile);
        }
    }
}


