using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Addresses;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IAuthorizationService _authorizationService;

        public AddressController(IAddressService addressService, IAuthorizationService authorizationService)
        {
            _addressService = addressService;
            _authorizationService = authorizationService;
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id") ?? User.FindFirstValue("sub");

            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, true, out var currentRole) &&
                idClaim != null && int.TryParse(idClaim, out var currentId))
            {
                return new CurrentUser { Id = currentId, Role = currentRole };
            }
            return null;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AddressDTO>>> GetByUserId(int userId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, userId, new AddressManagementRequirement(AddressOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var addresses = await _addressService.GetByUserIdAsync(userId);
            return Ok(addresses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDTO>> GetById(int id)
        {
            var address = await _addressService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, address, new AddressManagementRequirement(AddressOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(address);
        }

        [HttpPost]
        public async Task<ActionResult<AddressDTO>> Create([FromBody] CreateAddressDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new AddressManagementRequirement(AddressOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            var createdAddress = await _addressService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdAddress.Id }, createdAddress);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<AddressDTO>> Update(int id, [FromBody] UpdateAddressDTO dto)
        {
            var address = await _addressService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, address, new AddressManagementRequirement(AddressOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var updatedAddress = await _addressService.UpdateAsync(id, dto);
            return Ok(updatedAddress);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var address = await _addressService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, address, new AddressManagementRequirement(AddressOperation.Delete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _addressService.DeleteAsync(id);
            return Ok();
        }
    }
}
