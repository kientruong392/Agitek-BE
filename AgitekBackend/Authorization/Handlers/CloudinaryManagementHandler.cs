using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class CloudinaryManagementHandler : AuthorizationHandler<CloudinaryManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CloudinaryManagementRequirement requirement)
        {
            var isAllowed = requirement.Operation switch
            {
                CloudinaryOperation.Upload => IsInAnyRole(context, RoleName.Manager, RoleName.Admin, RoleName.SuperAdmin),
                CloudinaryOperation.UploadOrderProof => IsInAnyRole(context, RoleName.Delivery, RoleName.Admin, RoleName.SuperAdmin),
                _ => false
            };

            if (isAllowed)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }

        private static bool IsInAnyRole(AuthorizationHandlerContext context, params RoleName[] roles)
        {
            return roles.Any(role => context.User.IsInRole(role.ToString()));
        }
    }
}
