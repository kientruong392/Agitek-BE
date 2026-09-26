using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Models;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AgitekBackend.Authorization.Handlers
{
    public class ProductManagementHandler : AuthorizationHandler<ProductManagementRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ProductManagementHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ProductManagementRequirement requirement)
        {
            var resource = context.Resource;
            
            bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role");
            RoleName? currentRole = null;
            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, out var roleParsed))
            {
                currentRole = roleParsed;
            }

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue("id");
            int? currentUserId = null;
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            // 1. Super Admin & Admin: Full bypass
            if (currentRole == RoleName.SuperAdmin || currentRole == RoleName.Admin)
            {
                context.Succeed(requirement);
                return;
            }

            // Helper to get Product if resource is an ID
            Product? product = null;
            int? targetProductId = resource as int?;
            if (targetProductId != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var productDto = await productService.GetByIdAsync(targetProductId.Value);
                if (productDto != null)
                {
                    product = new Product 
                    { 
                        Id = productDto.Id, 
                        UserId = productDto.UserId, 
                        Status = productDto.Status 
                    };
                }
            }
            else if (resource is ProductDTO dto)
            {
                product = new Product { Id = dto.Id, UserId = dto.UserId, Status = dto.Status };
            }
            else if (resource is Product p)
            {
                product = p;
            }

            // 2. Manager Role
            if (currentRole == RoleName.Manager)
            {
                if (requirement.Operation == ProductOperation.Create || 
                    requirement.Operation == ProductOperation.ChangeStatus ||
                    requirement.Operation == ProductOperation.Delete ||
                    requirement.Operation == ProductOperation.SoftDelete ||
                    requirement.Operation == ProductOperation.HardDelete ||
                    requirement.Operation == ProductOperation.Read ||
                    requirement.Operation == ProductOperation.UpdateStockQuantity)
                {
                    context.Succeed(requirement);
                    return;
                }

                if (requirement.Operation == ProductOperation.Update)
                {
                    if (product != null && product.Status == ProductStatus.Deleted)
                    {
                        return; // Forbidden
                    }
                    context.Succeed(requirement);
                    return;
                }

                if (requirement.Operation == ProductOperation.UpdateQuantity)
                {
                    if (product != null && product.Status != ProductStatus.Pending && product.Status != ProductStatus.Deleted)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    return; // Forbidden
                }

                if (requirement.Operation == ProductOperation.UpdateInitialQuantity)
                {
                    // Tác giả tạo ra sản phẩm đó hoặc Manager được phép thực hiện
                    if (product != null && product.Status == ProductStatus.Pending)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    return;
                }


            }

            // 3. Staff Role
            if (currentRole == RoleName.Staff)
            {
                if (product != null && product.Status == ProductStatus.Deleted)
                {
                    return; // Forbidden all operations
                }

                if (requirement.Operation == ProductOperation.Create || requirement.Operation == ProductOperation.Read)
                {
                    context.Succeed(requirement);
                    return;
                }

                if (requirement.Operation == ProductOperation.Update)
                {
                    if (product != null && product.UserId == currentUserId && product.Status == ProductStatus.Pending)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    return; // Forbidden
                }

                if (requirement.Operation == ProductOperation.UpdateQuantity)
                {
                    if (product != null && product.Status != ProductStatus.Pending && product.Status != ProductStatus.Deleted)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    return; // Forbidden
                }

                if (requirement.Operation == ProductOperation.UpdateInitialQuantity)
                {
                    // Tác giả tạo ra sản phẩm đó hoặc Manager được phép thực hiện
                    if (product != null && product.UserId == currentUserId && product.Status == ProductStatus.Pending)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    return;
                }
                
                return; // Forbidden for ChangeStatus, Delete, SoftDelete, HardDelete, UpdateStockQuantity
            }

            // 4. Other Roles (Guest, Customer, Delivery)
            if (requirement.Operation == ProductOperation.Read)
            {
                if (product != null)
                {
                    if (product.Status != ProductStatus.Pending && 
                        product.Status != ProductStatus.Deleted && 
                        product.Status != ProductStatus.Hidden)
                    {
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
