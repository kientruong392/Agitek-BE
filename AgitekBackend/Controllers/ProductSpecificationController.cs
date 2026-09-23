using AgitekBackend.DTOs.ProductSpecifications;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductSpecificationController : ControllerBase
    {
        private readonly IProductSpecificationService _productSpecificationService;

        public ProductSpecificationController(IProductSpecificationService productSpecificationService)
        {
            _productSpecificationService = productSpecificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductSpecificationDTO>>> GetAll()
        {
            var specs = await _productSpecificationService.GetAllAsync();
            return Ok(specs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductSpecificationDTO>> GetById(int id)
        {
            var spec = await _productSpecificationService.GetByIdAsync(id);
            return Ok(spec);
        }

        [HttpPost]
        public async Task<ActionResult<ProductSpecificationDTO>> Create([FromBody] CreateProductSpecificationDTO dto)
        {
            var createdSpec = await _productSpecificationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdSpec.Id }, createdSpec);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ProductSpecificationDTO>> Update(int id, [FromBody] UpdateProductSpecificationDTO dto)
        {
            var updatedSpec = await _productSpecificationService.UpdateAsync(id, dto);
            return Ok(updatedSpec);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productSpecificationService.DeleteAsync(id);
            return Ok();
        }
    }
}


