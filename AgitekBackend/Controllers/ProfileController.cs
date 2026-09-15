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

        [HttpPost]
        public async Task<ActionResult<ProfileDTO>> Create([FromBody] CreateProfileDTO dto)
        {
            var createdProfile = await _profileService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { userId = createdProfile.UserId }, createdProfile);
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

        [HttpDelete("{userId}")]
        public async Task<ActionResult> Delete(int userId)
        {
            var success = await _profileService.DeleteAsync(userId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


