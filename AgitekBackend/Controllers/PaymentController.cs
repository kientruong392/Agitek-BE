using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IAuthorizationService _authorizationService;

        public PaymentController(IPaymentService paymentService, IAuthorizationService authorizationService)
        {
            _paymentService = paymentService;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> GetById(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var payment = await _paymentService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, payment, new PaymentManagementRequirement(PaymentOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetByOrderId(int orderId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, orderId, new PaymentManagementRequirement(PaymentOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access payments for this order");

            var payments = await _paymentService.GetByOrderIdAsync(orderId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> Create([FromBody] CreatePaymentDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.OrderId, new PaymentManagementRequirement(PaymentOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to create a payment for this order");

            var payment = await _paymentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
    }
}
