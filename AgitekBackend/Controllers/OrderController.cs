using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;using AgitekBackend.DTOs.Orders;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAuthorizationService _authorizationService;

        public OrderController(IOrderService orderService, IAuthorizationService authorizationService)
        {
            _orderService = orderService;
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
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAll([FromQuery] int? userId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var targetUserId = userId ?? currentUser.Id;
            if (targetUserId <= 0)
                throw new BadRequestException("User Id must be greater than 0");

            var authResult = await _authorizationService.AuthorizeAsync(
                User, targetUserId, new OrderManagementRequirement(OrderOperation.ReadAll));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access orders for this user");

            var orders = await _orderService.GetAllAsync(targetUserId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new OrderManagementRequirement(OrderOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var createdOrder = await _orderService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OrderDTO>> Update(int id, [FromBody] UpdateOrderDTO dto)
        {
            var order = await _orderService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            if (order!.Status != OrderStatus.Ordered)
                throw new ForbiddenException("Order can only be updated while in Ordered status");

            var updatedOrder = await _orderService.UpdateAsync(id, dto);
            return Ok(updatedOrder);
        }
    }
}
