using AgitekBackend.DTOs.LocationCoordinates;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationCoordinatesController : ControllerBase
    {
        private readonly ILocationCoordinatesService _locationCoordinatesService;

        public LocationCoordinatesController(ILocationCoordinatesService locationCoordinatesService)
        {
            _locationCoordinatesService = locationCoordinatesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationCoordinatesDTO>>> GetAll()
        {
            var locations = await _locationCoordinatesService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationCoordinatesDTO>> GetById(int id)
        {
            var location = await _locationCoordinatesService.GetByIdAsync(id);
            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<LocationCoordinatesDTO>> Create([FromBody] CreateLocationCoordinatesDTO dto)
        {
            var createdLocation = await _locationCoordinatesService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdLocation.Id }, createdLocation);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<LocationCoordinatesDTO>> Update(int id, [FromBody] UpdateLocationCoordinatesDTO dto)
        {
            var updatedLocation = await _locationCoordinatesService.UpdateAsync(id, dto);
            return Ok(updatedLocation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _locationCoordinatesService.DeleteAsync(id);
            return Ok();
        }
    }
}


