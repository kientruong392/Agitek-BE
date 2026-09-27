using AgitekBackend.DTOs.Locations;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetAll()
        {
            var locations = await _locationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<LocationDTO>> Create([FromBody] CreateLocationDTO dto)
        {
            var createdLocation = await _locationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdLocation.Id }, createdLocation);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<LocationDTO>> Update(int id, [FromBody] UpdateLocationDTO dto)
        {
            var updatedLocation = await _locationService.UpdateAsync(id, dto);
            return Ok(updatedLocation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _locationService.DeleteAsync(id);
            return Ok();
        }
    }
}
