using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class BrandManagementHandler : AuthorizationHandler<BrandManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BrandManagementRequirement requirement)
        {
            if (requirement.Operation == BrandOperation.Read)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, out var currentRole))
            {
                if (currentRole == RoleName.Manager || currentRole == RoleName.Admin || currentRole == RoleName.SuperAdmin)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
