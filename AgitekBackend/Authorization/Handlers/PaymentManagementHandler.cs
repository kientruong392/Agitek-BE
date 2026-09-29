using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class PaymentManagementHandler : AuthorizationHandler<PaymentManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PaymentManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, true, out var currentRole))
                return;

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return;

            // 1. Super Admin, Admin & Manager: full access to any user's payment
            if (currentRole == RoleName.SuperAdmin ||
                currentRole == RoleName.Admin ||
                currentRole == RoleName.Manager)
            {
                context.Succeed(requirement);
                return;
            }

            // 2. Create: only Customer (Guest excluded for now)
            if (requirement.Operation == PaymentOperation.Create)
            {
                if (currentRole == RoleName.Customer &&
                    await GetOrderOwnerIdAsync(context.Resource) == currentUserId)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            // 3. Read: only the owner of the order that the payment belongs to
            if (requirement.Operation == PaymentOperation.Read &&
                await GetOrderOwnerIdAsync(context.Resource) == currentUserId)
            {
                context.Succeed(requirement);
            }
        }

        private async Task<int?> GetOrderOwnerIdAsync(object? resource)
        {
            return resource switch
            {
                Payment payment => payment.Order?.UserId,
                PaymentDTO paymentDto => paymentDto.OrderUserId,
                Order order => order.UserId,
                int orderId => await GetOrderUserIdAsync(orderId),
                _ => null
            };
        }

        private async Task<int?> GetOrderUserIdAsync(int orderId)
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var orderDto = await orderService.GetByIdAsync(orderId);
            return orderDto?.UserId;
        }
    }
}
