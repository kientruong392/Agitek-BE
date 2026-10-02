using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Orders;
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

            var isDelivery = currentUser.Role == RoleName.Delivery;
            var targetUserId = isDelivery ? currentUser.Id : userId ?? currentUser.Id;
            var orders = await _orderService.GetAllAsync(targetUserId, assignedToDelivery: isDelivery);

            var orderList = orders.ToList();
            var authorizationResource = orderList.FirstOrDefault() ?? new OrderDTO
            {
                UserId = targetUserId,
                DeliveredByUserId = isDelivery ? currentUser.Id : null
            };

            var authResult = await _authorizationService.AuthorizeAsync(
                User, authorizationResource, new OrderManagementRequirement(OrderOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access these orders");

            return Ok(orderList);
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
                User, order, new OrderManagementRequirement(OrderOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            if (order.Status != OrderStatus.Ordered)
                throw new ForbiddenException("Order can only be updated while in Ordered status");

            return Ok(await _orderService.UpdateAsync(id, dto));
        }

        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<OrderDTO>> Cancel(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Cancel));
            if (!authResult.Succeeded)
                throw new ForbiddenException("Only the order owner can cancel this order");

            return Ok(await _orderService.CancelOrderAsync(id));
        }

        [HttpPatch("{id}/complete")]
        public async Task<ActionResult<OrderDTO>> Complete(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Complete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("Only the order owner can complete this order");

            return Ok(await _orderService.CompleteOrderAsync(id));
        }

        [HttpPatch("{id}/confirm")]
        public async Task<ActionResult<OrderDTO>> Confirm(int id, [FromBody] ConfirmOrderDTO dto)
        {
            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Confirm));
            if (!authResult.Succeeded)
                throw new ForbiddenException("Only management roles can confirm this order");

            return Ok(await _orderService.ConfirmOrderAsync(id, dto));
        }

        [HttpPatch("{id}/ship")]
        public async Task<ActionResult<OrderDTO>> Ship(int id, [FromBody] ShipOrderDTO dto)
        {
            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Ship));
            if (!authResult.Succeeded)
                throw new ForbiddenException("Only the assigned delivery user or administrators can ship this order");

            return Ok(await _orderService.ShipOrderAsync(id, dto));
        }
    }
}
