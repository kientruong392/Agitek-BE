using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Warranties;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarrantyController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;
        private readonly IAuthorizationService _authorizationService;

        public WarrantyController(IWarrantyService warrantyService, IAuthorizationService authorizationService)
        {
            _warrantyService = warrantyService;
            _authorizationService = authorizationService;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyDTO>>> GetAll()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new WarrantyManagementRequirement(WarrantyOperation.ReadAll));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var warranties = await _warrantyService.GetAllAsync(currentUser);
            return Ok(warranties);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<WarrantyDTO>>> GetMy()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            return Ok(await _warrantyService.GetMyAsync(currentUser.Id));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDTO>> GetById(int id)
        {
            var warranty = await _warrantyService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, warranty, new WarrantyManagementRequirement(WarrantyOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(warranty);
        }

        [HttpPost]
        public async Task<ActionResult<WarrantyDTO>> Create([FromBody] CreateWarrantyDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.OrderId, new WarrantyManagementRequirement(WarrantyOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var warranty = await _warrantyService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = warranty.Id }, warranty);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<WarrantyDTO>> Update(int id, [FromBody] UpdateWarrantyDTO dto)
        {
            var warranty = await _warrantyService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, warranty, new WarrantyManagementRequirement(WarrantyOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var updatedWarranty = await _warrantyService.UpdateAsync(id, dto);
            return Ok(updatedWarranty);
        }

        [HttpPatch("{id}/approve")]
        public async Task<ActionResult<WarrantyDTO>> Approve(int id, [FromBody] ApproveWarrantyDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new WarrantyManagementRequirement(WarrantyOperation.Approve));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var warranty = await _warrantyService.ApproveAsync(id, currentUser.Id, dto);
            return Ok(warranty);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var warranty = await _warrantyService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, warranty, new WarrantyManagementRequirement(WarrantyOperation.Delete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _warrantyService.DeleteAsync(id);
            return Ok();
        }
    }
}
