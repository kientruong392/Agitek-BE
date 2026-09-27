using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Addresses;
using AgitekBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class AddressManagementHandler : AuthorizationHandler<AddressManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AddressManagementRequirement requirement)
        {
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim == null || !Enum.TryParse<RoleName>(roleClaim, out var currentRole))
                return Task.CompletedTask;

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return Task.CompletedTask;

            // 1. Create: only Customer (Guest excluded for now)
            if (requirement.Operation == AddressOperation.Create)
            {
                if (currentRole == RoleName.Customer)
                {
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }

            // 2. Admin & SuperAdmin: full Read access to any user's address
            if (requirement.Operation == AddressOperation.Read &&
                (currentRole == RoleName.Admin || currentRole == RoleName.SuperAdmin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 3. Read/Update/Delete: only the owner of the resource
            var targetUserId = context.Resource switch
            {
                AddressDTO addressDto => addressDto.UserId,
                Address address => address.UserId,
                int userId => userId,
                _ => (int?)null
            };

            if (targetUserId.HasValue && targetUserId.Value == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
