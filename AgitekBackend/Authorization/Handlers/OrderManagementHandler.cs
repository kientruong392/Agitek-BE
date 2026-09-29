using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class OrderManagementHandler : AuthorizationHandler<OrderManagementRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OrderManagementRequirement requirement)
        {
            await Task.Yield();

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, true, out var currentRole))
                return;

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return;

            if (requirement.Operation == OrderOperation.Create)
            {
                if (currentRole == RoleName.Customer)
                    context.Succeed(requirement);
                return;
            }

            var targetUserId = context.Resource switch
            {
                OrderDTO orderDto => orderDto.UserId,
                Order order => order.UserId,
                int userId => userId,
                _ => (int?)null
            };

            if (requirement.Operation == OrderOperation.ReadAll)
            {
                if (targetUserId == currentUserId ||
                    currentRole == RoleName.Manager ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            if (requirement.Operation == OrderOperation.Read &&
                (currentRole == RoleName.Staff ||
                 currentRole == RoleName.Manager ||
                 currentRole == RoleName.Admin ||
                 currentRole == RoleName.SuperAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            if (targetUserId.HasValue && targetUserId.Value == currentUserId)
                context.Succeed(requirement);
        }
    }
}
