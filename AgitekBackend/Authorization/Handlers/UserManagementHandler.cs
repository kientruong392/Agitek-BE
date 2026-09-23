using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class UserManagementHandler : AuthorizationHandler<UserManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UserManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserManagementRequirement requirement)
        {
            var resource = context.Resource;
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, out var currentRole))
                return;

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return;

            int? targetUserIdInitial = resource as int?;
            if (targetUserIdInitial == null && resource is User targetUserObjInit)
            {
                targetUserIdInitial = targetUserObjInit.Id;
            }

            if ((requirement.Operation == UserOperation.Update || requirement.Operation == UserOperation.Delete) 
                && targetUserIdInitial == currentUserId)
            {
                return;
            }

            if (currentRole == RoleName.SuperAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            if (currentRole != RoleName.Admin)
                return;

            if (requirement.Operation == UserOperation.Read)
            {
                context.Succeed(requirement);
                return;
            }

            if (requirement.Operation == UserOperation.Create)
            {
                if (resource is RoleName targetRole)
                {
                    if (targetRole == RoleName.SuperAdmin || targetRole == RoleName.Admin)
                        return;
                }
                context.Succeed(requirement);
                return;
            }

            int? targetUserId = resource as int?;
            
            if (targetUserId == null && resource is User targetUserObj)
            {
                targetUserId = targetUserObj.Id;
            }

            if (targetUserId == null)
            {
                return;
            }

            if (targetUserId == currentUserId)
            {
                return;
            }

            RoleName? targetUserRole = null;

            if (resource is User userModel)
            {
                targetUserRole = userModel.Role;
            }
            else
            {
                using var scope = _scopeFactory.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var dbUser = await userService.GetByIdAsync(targetUserId.Value);
                if (dbUser == null)
                {
                    context.Succeed(requirement);
                    return;
                }
                targetUserRole = dbUser.Role;
            }

            if (targetUserRole == RoleName.SuperAdmin || targetUserRole == RoleName.Admin)
                return;

            context.Succeed(requirement);
        }
    }
}
