using AgitekBackend.DTOs.Medias;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaDTO>>> GetAll()
        {
            var medias = await _mediaService.GetAllAsync();
            return Ok(medias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MediaDTO>> GetById(int id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            return Ok(media);
        }

        [HttpPost]
        public async Task<ActionResult<MediaDTO>> Create([FromBody] CreateMediaDTO dto)
        {
            var createdMedia = await _mediaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdMedia.Id }, createdMedia);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<MediaDTO>> Update(int id, [FromBody] UpdateMediaDTO dto)
        {
            var updatedMedia = await _mediaService.UpdateAsync(id, dto);
            return Ok(updatedMedia);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _mediaService.DeleteAsync(id);
            return Ok();
        }
    }
}


