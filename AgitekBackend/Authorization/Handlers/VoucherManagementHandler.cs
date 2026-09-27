using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class VoucherManagementHandler : AuthorizationHandler<VoucherManagementRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            VoucherManagementRequirement requirement)
        {
            var resource = context.Resource;

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            RoleName? currentRole = null;
            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, out var roleParsed))
            {
                currentRole = roleParsed;
            }

            // 1. Super Admin & Admin: Full bypass
            if (currentRole == RoleName.SuperAdmin || currentRole == RoleName.Admin)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2. Manager Role
            if (currentRole == RoleName.Manager)
            {
                if (requirement.Operation == VoucherOperation.Create ||
                    requirement.Operation == VoucherOperation.Update ||
                    requirement.Operation == VoucherOperation.Read)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            // 3. Other Roles
            if (requirement.Operation == VoucherOperation.Read)
            {
                var status = resource switch
                {
                    VoucherDTO voucherDto => voucherDto.Status,
                    Voucher voucher => voucher.Status,
                    _ => VoucherStatus.Active
                };

                if (status != VoucherStatus.Inactive)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
