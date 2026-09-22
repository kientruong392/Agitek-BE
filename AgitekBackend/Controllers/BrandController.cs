using AgitekBackend.DTOs.Brands;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDTO>>> GetAll()
        {
            var brands = await _brandService.GetAllAsync();
            return Ok(brands);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDTO>> GetById(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return Ok(brand);
        }

        [HttpPost]
        public async Task<ActionResult<BrandDTO>> Create([FromBody] CreateBrandDTO dto)
        {
            var createdBrand = await _brandService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdBrand.Id }, createdBrand);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<BrandDTO>> Update(int id, [FromBody] UpdateBrandDTO dto)
        {
            var updatedBrand = await _brandService.UpdateAsync(id, dto);
            if (updatedBrand == null)
            {
                return NotFound();
            }
            return Ok(updatedBrand);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _brandService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


