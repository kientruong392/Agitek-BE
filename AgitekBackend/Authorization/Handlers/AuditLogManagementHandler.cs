using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class AuditLogManagementHandler : AuthorizationHandler<AuditLogManagementRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuditLogManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (Enum.TryParse<RoleName>(roleClaim, true, out var role) &&
                (role == RoleName.Manager || role == RoleName.Admin || role == RoleName.SuperAdmin))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
