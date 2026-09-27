using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Locations;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IAuthorizationService _authorizationService;

        public LocationController(ILocationService locationService, IAuthorizationService authorizationService)
        {
            _locationService = locationService;
            _authorizationService = authorizationService;
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id") ?? User.FindFirstValue("sub");

            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, out var currentRole) &&
                idClaim != null && int.TryParse(idClaim, out var currentId))
            {
                return new CurrentUser { Id = currentId, Role = currentRole };
            }
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetAll()
        {
            var locations = await _locationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<LocationDTO>> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            return Ok(location);
        }

        [HttpGet("{id}/children")]
        [AllowAnonymous]
        public async Task<ActionResult<LocationDTO>> GetByIdWithChildrens(int id)
        {
            var location = await _locationService.GetByIdWithChildrensAsync(id);
            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<LocationDTO>> Create([FromBody] CreateLocationDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new LocationManagementRequirement(LocationOperation.Create));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var createdLocation = await _locationService.CreateAsync(dto, currentUser);
            return CreatedAtAction(nameof(GetById), new { id = createdLocation.Id }, createdLocation);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<LocationDTO>> Update(int id, [FromBody] UpdateLocationDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new LocationManagementRequirement(LocationOperation.Update));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var updatedLocation = await _locationService.UpdateAsync(id, dto, currentUser);
            return Ok(updatedLocation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new LocationManagementRequirement(LocationOperation.Delete));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            await _locationService.DeleteAsync(id, currentUser);
            return Ok();
        }
    }
}
