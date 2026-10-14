using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
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
    [Authorize]
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

        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetCustomers()
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var users = await _userService.GetAllCustomersAsync();
            return Ok(users);
        }

        [HttpGet("staff")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetStaff()
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var users = await _userService.GetAllStaffAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new UserManagementRequirement(UserOperation.Read));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpPost("create-staff-account")]
        public async Task<ActionResult<UserDTO>> CreateStaffAccount([FromBody] CreateStaffDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.Role, new UserManagementRequirement(UserOperation.Create));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized();

            var createdUser = await _userService.CreateStaffAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPatch("{id}/role")]
        public async Task<ActionResult<UserDTO>> UpdateRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new UserManagementRequirement(UserOperation.Update));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Unauthorized();

            var currentRoleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            if (!Enum.TryParse<RoleName>(currentRoleClaim, out var currentRole))
                throw new ForbiddenException("You do not have permission to access this resource");

            var updatedUser = await _userService.UpdateRoleAsync(id, dto, currentUserId, currentRole);
            return Ok(updatedUser);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<UserDTO>> UpdateStatus(int id, [FromBody] UpdateUserStatusDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new UserManagementRequirement(UserOperation.Update));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var updatedUser = await _userService.UpdateStatusAsync(id, dto);
            return Ok(updatedUser);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDTO>> GetMe()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();
            return Ok(await _userService.GetMeAsync(currentUser.Id));
        }

        [HttpPatch("my-account")]
        public async Task<ActionResult<UserDTO>> UpdateMyAccount([FromBody] UpdateMyAccountDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var updatedUser = await _userService.UpdateAccountAsync(currentUser.Id, dto);
            return Ok(updatedUser);
        }

        [HttpPatch("my-account/password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            await _userService.ChangeMyPasswordAsync(currentUser.Id, dto.OldPassword, dto.NewPassword);
            return NoContent();
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("id")
                ?? User.FindFirstValue("sub");

            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, true, out var role) &&
                idClaim != null && int.TryParse(idClaim, out var id))
            {
                return new CurrentUser { Id = id, Role = role };
            }

            return null;
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new UserManagementRequirement(UserOperation.Delete));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _userService.DeleteAsync(id);
            return Ok();
        }
    }
}
