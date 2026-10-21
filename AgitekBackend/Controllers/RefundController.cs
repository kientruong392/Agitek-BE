using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Refunds;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly IAuthorizationService _authorizationService;

        public RefundController(IRefundService refundService, IAuthorizationService authorizationService)
        {
            _refundService = refundService;
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
        public async Task<ActionResult<IEnumerable<RefundDTO>>> GetAll()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new RefundManagementRequirement(RefundOperation.ReadAll));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access refund requests");

            var refunds = await _refundService.GetAllAsync();
            return Ok(refunds);
        }

        [HttpGet("{orderId}/{productId}")]
        public async Task<ActionResult<RefundDTO>> GetById(int orderId, int productId)
        {
            var refund = await _refundService.GetByIdAsync(orderId, productId);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, refund, new RefundManagementRequirement(RefundOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this refund request");

            return Ok(refund);
        }

        [HttpPost]
        public async Task<ActionResult<RefundDTO>> Create([FromBody] CreateRefundDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.OrderId, new RefundManagementRequirement(RefundOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to create a refund for this order");

            var refund = await _refundService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { refund.OrderId, refund.ProductId }, refund);
        }

        [HttpPatch("{orderId}/{productId}")]
        public async Task<ActionResult<RefundDTO>> Update(int orderId, int productId, [FromBody] ApproveRefundDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new RefundManagementRequirement(RefundOperation.Approve));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to approve a refund");

            var refund = await _refundService.UpdateAsync(orderId, productId, currentUser.Id, dto);
            return Ok(refund);
        }
    }
}
