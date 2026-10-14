using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Profiles;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IAuthorizationService _authorizationService;

        public ProfileController(IProfileService profileService, IAuthorizationService authorizationService)
        {
            _profileService = profileService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfileDTO>>> GetAll()
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(await _profileService.GetAllAsync());
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<ProfileDTO>> GetById(int userId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, userId, new UserManagementRequirement(UserOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(await _profileService.GetByUserIdAsync(userId));
        }

        [HttpPatch("{userId:int}")]
        public async Task<ActionResult<ProfileDTO>> Update(int userId, [FromBody] UpdateProfileDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, userId, new UserManagementRequirement(UserOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(await _profileService.UpdateAsync(userId, dto));
        }
    }
}
