using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Brands;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly IAuthorizationService _authorizationService;

        public BrandController(IBrandService brandService, IAuthorizationService authorizationService)
        {
            _brandService = brandService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BrandDTO>>> GetAll()
        {
            var brands = await _brandService.GetAllAsync();
            return Ok(brands);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BrandDTO>> GetById(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            return Ok(brand);
        }

        [HttpGet("{id}/products")]
        [AllowAnonymous]
        public async Task<ActionResult<BrandDTO>> GetByIdWithProducts(int id)
        {
            var brand = await _brandService.GetByIdWithProductsAsync(id);
            return Ok(brand);
        }

        [HttpGet("slug/{slug}/products")]
        [AllowAnonymous]
        public async Task<ActionResult<BrandDTO>> GetBySlugWithProducts(string slug)
        {
            var brand = await _brandService.GetBySlugWithProductsAsync(slug);
            return Ok(brand);
        }

        [HttpPost]
        public async Task<ActionResult<BrandDTO>> Create([FromBody] CreateBrandDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new BrandManagementRequirement(BrandOperation.Create));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var createdBrand = await _brandService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdBrand.Id }, createdBrand);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<BrandDTO>> Update(int id, [FromBody] UpdateBrandDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new BrandManagementRequirement(BrandOperation.Update));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var updatedBrand = await _brandService.UpdateAsync(id, dto);
            return Ok(updatedBrand);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new BrandManagementRequirement(BrandOperation.Delete));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            await _brandService.DeleteAsync(id);
            return Ok();
        }
    }
}


