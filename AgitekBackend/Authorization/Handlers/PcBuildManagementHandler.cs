using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Authorization.Handlers
{
    public class PcBuildManagementHandler : AuthorizationHandler<PcBuildManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PcBuildManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PcBuildManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (context.User.Identity?.IsAuthenticated != true
                || !Enum.TryParse<RoleName>(roleClaim, true, out var role)
                || role != RoleName.Customer
                || !int.TryParse(userIdClaim, out var userId)
                || userId <= 0)
                return;

            if (requirement.Operation is PcBuildOperation.ReadAll or PcBuildOperation.Create)
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is not int buildId || buildId <= 0) return;
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (await db.PcBuilds.AnyAsync(build => build.Id == buildId && build.UserId == userId))
                context.Succeed(requirement);
        }
    }
}
