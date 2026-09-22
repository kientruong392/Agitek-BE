using AgitekBackend.DTOs.Warranties;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;

        public WarrantyController(IWarrantyService warrantyService)
        {
            _warrantyService = warrantyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarrantyDTO>>> GetAll()
        {
            var warranties = await _warrantyService.GetAllAsync();
            return Ok(warranties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarrantyDTO>> GetById(int id)
        {
            var warranty = await _warrantyService.GetByIdAsync(id);
            if (warranty == null)
            {
                return NotFound();
            }
            return Ok(warranty);
        }

        [HttpPost]
        public async Task<ActionResult<WarrantyDTO>> Create([FromBody] CreateWarrantyDTO dto)
        {
            var createdWarranty = await _warrantyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdWarranty.Id }, createdWarranty);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<WarrantyDTO>> Update(int id, [FromBody] UpdateWarrantyDTO dto)
        {
            var updatedWarranty = await _warrantyService.UpdateAsync(id, dto);
            if (updatedWarranty == null)
            {
                return NotFound();
            }
            return Ok(updatedWarranty);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _warrantyService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}


