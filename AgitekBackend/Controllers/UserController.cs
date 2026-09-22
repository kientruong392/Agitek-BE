using AgitekBackend.Authorization.Requirements;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgitekBackend.Common;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication by default
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthorizationService authorizationService, IAuthService authService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            
            if (!authResult.Succeeded)
                return Forbid();

            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            
            if (!authResult.Succeeded)
                return Forbid();

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("create-staff-account")]
        public async Task<ActionResult<UserDTO>> CreateStaffAccount([FromBody] CreateStaffDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.Role, new UserManagementRequirement(UserOperation.Create));
            
            if (!authResult.Succeeded)
                return Forbid();

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized();

            var createdUser = await _authService.CreateStaffAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserDTO>> Update(int id, [FromBody] UpdateUserDTO dto)
        {
            // Pass the target user ID as the resource for dynamic authorization
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new UserManagementRequirement(UserOperation.Update));
            
            if (!authResult.Succeeded)
                return Forbid();

            var currentRoleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            if (!Enum.TryParse<RoleName>(currentRoleClaim, out var currentRole))
                return Forbid();

            var updatedUser = await _userService.UpdateAsync(id, dto, currentRole);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(updatedUser);
        }

        [HttpPatch("my-account")]
        public async Task<ActionResult<UserDTO>> UpdateMyAccount([FromBody] UpdateMyAccountDTO dto)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized();

            var updatedUser = await _userService.UpdateAccountAsync(currentUserId, dto);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // Pass the target user ID as the resource for dynamic authorization
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new UserManagementRequirement(UserOperation.Delete));
            
            if (!authResult.Succeeded)
                return Forbid();

            var success = await _userService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
