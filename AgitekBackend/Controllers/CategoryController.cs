using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Categories;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IAuthorizationService _authorizationService;

        public CategoryController(ICategoryService categoryService, IAuthorizationService authorizationService)
        {
            _categoryService = categoryService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDTO>> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        [HttpGet("with-subcategories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllWithSubCategories()
        {
            var categories = await _categoryService.GetAllWithSubCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{parentCategoryId}/subcategories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllSubCategories(int parentCategoryId)
        {
            var subCategories = await _categoryService.GetAllSubCategoriesAsync(parentCategoryId);
            return Ok(subCategories);
        }

        [HttpGet("{id}/products")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDTO>> GetByIdWithProducts(int id)
        {
            var category = await _categoryService.GetByIdWithProducts(id);
            return Ok(category);
        }

        [HttpGet("slug/{slug}/products")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDTO>> GetBySlugWithProducts(string slug)
        {
            var category = await _categoryService.GetBySlugWithProductsAsync(slug);
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> Create([FromBody] CreateCategoryDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new CategoryManagementRequirement(CategoryOperation.Create));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var createdCategory = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<CategoryDTO>> Update(int id, [FromBody] UpdateCategoryDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new CategoryManagementRequirement(CategoryOperation.Update));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            var updatedCategory = await _categoryService.UpdateAsync(id, dto);
            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, null, new CategoryManagementRequirement(CategoryOperation.Delete));
            if (!authResult.Succeeded) throw new ForbiddenException("You do not have permission to access this resource");

            await _categoryService.DeleteAsync(id);
            return Ok();
        }
    }
}


