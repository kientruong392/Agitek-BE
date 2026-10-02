using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Warranties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Authorization.Handlers
{
    public class WarrantyManagementHandler : AuthorizationHandler<WarrantyManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public WarrantyManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WarrantyManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, true, out var currentRole))
                return;

            if (requirement.Operation == WarrantyOperation.ReadAll)
            {
                if (currentRole == RoleName.SuperAdmin ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.Manager ||
                    currentRole == RoleName.Staff)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            if (requirement.Operation == WarrantyOperation.Approve)
            {
                if (currentRole == RoleName.Staff ||
                    currentRole == RoleName.Manager ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return;

            var ownerId = await GetWarrantyOwnerIdAsync(context.Resource);

            if (requirement.Operation == WarrantyOperation.Read)
            {
                if (currentRole == RoleName.Staff ||
                    currentRole == RoleName.Manager ||
                    currentRole == RoleName.Admin ||
                    currentRole == RoleName.SuperAdmin ||
                    ownerId == currentUserId)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            if (requirement.Operation == WarrantyOperation.Create ||
                requirement.Operation == WarrantyOperation.Update ||
                requirement.Operation == WarrantyOperation.Delete)
            {
                if (ownerId == currentUserId)
                    context.Succeed(requirement);

                return;
            }
        }

        private async Task<int?> GetWarrantyOwnerIdAsync(object? resource)
        {
            return resource switch
            {
                WarrantyDTO warrantyDto => warrantyDto.UserId,
                int orderId => await GetOrderOwnerIdAsync(orderId),
                _ => null
            };
        }

        private async Task<int?> GetOrderOwnerIdAsync(int orderId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.Orders
                .Where(o => o.Id == orderId)
                .Select(o => (int?)o.UserId)
                .FirstOrDefaultAsync();
        }
    }
}
