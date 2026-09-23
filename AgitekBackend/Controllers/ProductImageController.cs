using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductImageDTO>>> GetAll()
        {
            var images = await _productImageService.GetAllAsync();
            return Ok(images);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductImageDTO>> GetById(int id)
        {
            var image = await _productImageService.GetByIdAsync(id);
            return Ok(image);
        }

        [HttpPost]
        public async Task<ActionResult<ProductImageDTO>> Create([FromBody] CreateProductImageDTO dto)
        {
            var createdImage = await _productImageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdImage.Id }, createdImage);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ProductImageDTO>> Update(int id, [FromBody] UpdateProductImageDTO dto)
        {
            var updatedImage = await _productImageService.UpdateAsync(id, dto);
            return Ok(updatedImage);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productImageService.DeleteAsync(id);
            return Ok();
        }
    }
}


