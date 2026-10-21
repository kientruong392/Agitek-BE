using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.OrderTrackingLogs;
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

        public OrderController(
            IOrderService orderService,
            IAuthorizationService authorizationService)
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
        [Authorize(Roles = "Staff,Manager,Admin,SuperAdmin,Delivery,Customer,Guest")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAll()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var hasManagementScope = currentUser.Role == RoleName.Staff ||
                                     currentUser.Role == RoleName.Manager ||
                                     currentUser.Role == RoleName.Admin ||
                                     currentUser.Role == RoleName.SuperAdmin;
            int? userId = hasManagementScope ? null : currentUser.Id;
            var assignedToDelivery = currentUser.Role == RoleName.Delivery;
            return Ok(await _orderService.GetAllAsync(userId, assignedToDelivery));
        }


        [HttpGet("{id}/tracking")]
        public async Task<ActionResult<IEnumerable<OrderTrackingLogDTO>>> GetTrackingLogs(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Read));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");
            return Ok(await _orderService.GetTrackingLogsAsync(id));
        }

        [HttpPost("{id}/tracking")]
        [Authorize(Roles = "Delivery,Admin,SuperAdmin")]
        public async Task<ActionResult<IEnumerable<OrderTrackingLogDTO>>> AddTrackingLog(
            int id,
            [FromBody] CreateOrderTrackingLogDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return Ok(await _orderService.AddTrackingLogAsync(id, dto, currentUser));
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

        [HttpGet("delivery-users")]
        [Authorize(Roles = "Staff,Manager,Admin,SuperAdmin")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeliveryUsers()
        {
            var users = await _orderService.GetDeliveryUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var createdOrder = await _orderService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPost("guest")]
        [AllowAnonymous]
        public async Task<ActionResult<OrderDTO>> CreateGuest([FromBody] CreateGuestOrderDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var createdOrder = await _orderService.CreateGuestOrderAsync(dto);
            return StatusCode(StatusCodes.Status201Created, createdOrder);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OrderDTO>> Update(int id, [FromBody] UpdateOrderDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var order = await _orderService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(
                User, order, new OrderManagementRequirement(OrderOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to update this order");

            if (currentUser.Id != order.UserId && currentUser.Role != RoleName.Admin &&
                currentUser.Role != RoleName.SuperAdmin && currentUser.Role != RoleName.Manager)
                throw new ForbiddenException("You do not have permission to update this order");
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

        [HttpPatch("{id}/in-transit")]
        [Authorize(Roles = "Delivery,Admin,SuperAdmin")]
        public async Task<ActionResult<OrderDTO>> StartTransit(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();
            return Ok(await _orderService.StartTransitAsync(id, currentUser));
        }

        [HttpPatch("{id}/ship")]
        [Authorize(Roles = "Delivery,Admin,SuperAdmin")]
        public async Task<ActionResult<OrderDTO>> Ship(int id, [FromBody] ShipOrderDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var order = await _orderService.GetByIdAsync(id);

            if (currentUser.Role == RoleName.Delivery && order.DeliveredByUserId != currentUser.Id)
                throw new ForbiddenException("Only the assigned delivery user can ship this order");

            return Ok(await _orderService.ShipOrderAsync(id, dto));
        }
    }
}
