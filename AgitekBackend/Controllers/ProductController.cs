using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Products;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IAuthorizationService _authorizationService;

        public ProductController(IProductService productService, IAuthorizationService authorizationService)
        {
            _productService = productService;
            _authorizationService = authorizationService;
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id") ?? User.FindFirstValue("sub");
            
            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, out var currentRole) &&
                idClaim != null && int.TryParse(idClaim, out var currentId))
            {
                return new CurrentUser { Id = currentId, Role = currentRole };
            }
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAll()
        {
            var products = await _productService.GetAllAsync(GetCurrentUser());
            return Ok(products);
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFeatured([FromQuery] int count = 4)
        {
            var products = await _productService.GetFeaturedAsync(Math.Clamp(count, 1, 24), GetCurrentUser());
            return Ok(products);
        }

        [HttpGet("newest")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetNewest([FromQuery] int count = 4)
        {
            var products = await _productService.GetNewestAsync(Math.Clamp(count, 1, 24), GetCurrentUser());
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId, GetCurrentUser());
            return Ok(products);
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDTO>> GetBySlug(string slug)
        {
            var product = await _productService.GetBySlugAsync(slug);
            return Ok(product);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDTO>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            
            var authResult = await _authorizationService.AuthorizeAsync(
                User, product, new ProductManagementRequirement(ProductOperation.Read));
            
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, null, new ProductManagementRequirement(ProductOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var createdProduct = await _productService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ProductDTO>> Update(int id, [FromBody] UpdateProductDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            var result = await _productService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> ChangeStatus(int id, [FromBody] UpdateProductStatusDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.ChangeStatus));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.ChangeStatusAsync(id, dto.Status);
            return Ok();
        }

        [HttpPatch("{id}/quantity")]
        public async Task<ActionResult> UpdateQuantity(int id, [FromBody] UpdateProductQuantityDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.UpdateQuantity));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.UpdateQuantityAsync(id, dto.StockQuantity);
            return Ok();
        }

        [HttpPatch("{id}/quantity/initial")]
        public async Task<ActionResult> UpdateInitialQuantity(int id, [FromBody] UpdateProductQuantityDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.UpdateInitialQuantity));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.UpdateInitialQuantityAsync(id, dto.StockQuantity);
            return Ok();
        }

        [HttpPatch("{id}/quantity/stock")]
        public async Task<ActionResult> UpdateStockQuantity(int id, [FromBody] UpdateProductQuantityDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.UpdateStockQuantity));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.UpdateStockQuantityAsync(id, dto.StockQuantity);
            return Ok();
        }

        [HttpDelete("{id}/soft")]
        public async Task<ActionResult> SoftDelete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.SoftDelete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.SoftDeleteAsync(id);
            return Ok();
        }

        [HttpDelete("{id}/hard")]
        public async Task<ActionResult> HardDelete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.HardDelete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.HardDeleteAsync(id);
            return Ok();
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult> Restore(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ProductManagementRequirement(ProductOperation.SoftDelete));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to access this resource");

            await _productService.RestoreAsync(id);
            return Ok();
        }
    }
}
