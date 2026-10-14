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
            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out var currentUserId))
                return Task.CompletedTask;

            // Every authenticated user may create their own address.
            if (requirement.Operation == AddressOperation.Create)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, true, out var currentRole) &&
                requirement.Operation == AddressOperation.Read &&
                (currentRole == RoleName.Admin || currentRole == RoleName.SuperAdmin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Read, Update, and Delete remain limited to the owner.
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
