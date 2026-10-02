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

            var targetOrder = GetOrder(context.Resource);

            if (requirement.Operation == OrderOperation.Read)
            {
                if (currentRole == RoleName.Customer && targetOrder?.UserId == currentUserId ||
                    currentRole == RoleName.Staff ||
                    currentRole == RoleName.Manager ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin ||
                    currentRole == RoleName.Delivery && targetOrder?.DeliveredByUserId == currentUserId)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            if (requirement.Operation == OrderOperation.Cancel)
            {
                if (targetOrder?.UserId == currentUserId)
                    context.Succeed(requirement);
                return;
            }

            if (requirement.Operation == OrderOperation.Complete)
            {
                if (targetOrder?.UserId == currentUserId)
                    context.Succeed(requirement);
                return;
            }

            if (requirement.Operation == OrderOperation.Confirm)
            {
                if (currentRole == RoleName.Manager ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin)
                {
                    context.Succeed(requirement);
                }
                return;
            }

            if (requirement.Operation == OrderOperation.Ship)
            {
                if (currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin ||
                    currentRole == RoleName.Delivery && targetOrder?.DeliveredByUserId == currentUserId)
                {
                    context.Succeed(requirement);
                }
            }
        }

        private static Order? GetOrder(object? resource)
        {
            return resource switch
            {
                Order order => order,
                OrderDTO orderDto => new Order
                {
                    Id = orderDto.Id,
                    UserId = orderDto.UserId,
                    DeliveredByUserId = orderDto.DeliveredByUserId,
                    Status = orderDto.Status
                },
                _ => null
            };
        }
    }
}
