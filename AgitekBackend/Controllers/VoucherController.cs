using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly IAuthorizationService _authorizationService;

        public VoucherController(
            IVoucherService voucherService,
            IAuthorizationService authorizationService)
        {
            _voucherService = voucherService;
            _authorizationService = authorizationService;
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("id")
                ?? User.FindFirstValue("sub");

            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, true, out var currentRole) &&
                idClaim != null && int.TryParse(idClaim, out var currentId))
            {
                return new CurrentUser { Id = currentId, Role = currentRole };
            }

            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VoucherDTO>>> GetAll()
        {
            var currentUser = GetCurrentUser();
            var vouchers = await _voucherService.GetAllAsync(currentUser);
            return Ok(vouchers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VoucherDTO>> GetById(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User,
                voucher,
                new VoucherManagementRequirement(VoucherOperation.Read));

            if (!authResult.Succeeded)
            {
                throw new ForbiddenException("You do not have permission to access this resource");
            }

            return Ok(voucher);
        }

        [HttpPost]
        public async Task<ActionResult<VoucherDTO>> Create([FromBody] CreateVoucherDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User,
                null,
                new VoucherManagementRequirement(VoucherOperation.Create));

            if (!authResult.Succeeded)
            {
                throw new ForbiddenException("You do not have permission to access this resource");
            }

            var createdVoucher = await _voucherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdVoucher.Id }, createdVoucher);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<VoucherDTO>> Update(
            int id,
            [FromBody] UpdateVoucherDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User,
                id,
                new VoucherManagementRequirement(VoucherOperation.Update));

            if (!authResult.Succeeded)
            {
                throw new ForbiddenException("You do not have permission to access this resource");
            }

            var updatedVoucher = await _voucherService.UpdateAsync(id, dto);
            return Ok(updatedVoucher);
        }
    }
}
