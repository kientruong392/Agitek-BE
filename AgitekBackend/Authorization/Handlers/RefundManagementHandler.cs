using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Authorization.Handlers
{
    public class RefundManagementHandler : AuthorizationHandler<RefundManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RefundManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RefundManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, true, out var currentRole))
                return;

            if (requirement.Operation == RefundOperation.Approve)
            {
                if (currentRole == RoleName.Manager ||
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

            if (requirement.Operation == RefundOperation.Create &&
                context.Resource is int orderId &&
                await GetOrderOwnerIdAsync(orderId) == currentUserId)
            {
                context.Succeed(requirement);
            }
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
