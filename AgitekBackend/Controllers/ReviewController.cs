using AgitekBackend.DTOs.Reviews;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetAll()
        {
            var reviews = await _reviewService.GetAllAsync();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetById(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            return Ok(review);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDTO>> Create([FromBody] CreateReviewDTO dto)
        {
            var createdReview = await _reviewService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdReview.Id }, createdReview);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ReviewDTO>> Update(int id, [FromBody] UpdateReviewDTO dto)
        {
            var updatedReview = await _reviewService.UpdateAsync(id, dto);
            return Ok(updatedReview);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _reviewService.DeleteAsync(id);
            return Ok();
        }
    }
}


