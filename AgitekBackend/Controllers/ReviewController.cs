using System.Security.Claims;
using AgitekBackend.Authorization.Requirements;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Reviews;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IAuthorizationService _authorizationService;

        public ReviewController(IReviewService reviewService, IAuthorizationService authorizationService)
        {
            _reviewService = reviewService;
            _authorizationService = authorizationService;
        }

        private CurrentUser? GetCurrentUser()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("id")
                ?? User.FindFirstValue("sub");

            if (roleClaim != null && Enum.TryParse<RoleName>(roleClaim, true, out var role) &&
                idClaim != null && int.TryParse(idClaim, out var id))
            {
                return new CurrentUser { Id = id, Role = role };
            }

            return null;
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetAll(int productId)
        {
            var reviews = await _reviewService.GetAllAsync(productId);
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
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, dto.OrderId, new ReviewManagementRequirement(ReviewOperation.Create));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to create a review for this order");

            var review = await _reviewService.CreateAsync(dto, currentUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ReviewDTO>> Update(int id, [FromBody] UpdateReviewDTO dto)
        {
            var review = await _reviewService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(
                User, review, new ReviewManagementRequirement(ReviewOperation.Update));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to update this review");

            var updatedReview = await _reviewService.UpdateAsync(id, dto);
            return Ok(updatedReview);
        }

        [HttpPatch("{id}/reply")]
        public async Task<ActionResult<ReviewDTO>> Reply(int id, [FromBody] ReplyReviewDTO dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var authResult = await _authorizationService.AuthorizeAsync(
                User, id, new ReviewManagementRequirement(ReviewOperation.Reply));
            if (!authResult.Succeeded)
                throw new ForbiddenException("You do not have permission to reply to this review");

            var review = await _reviewService.ReplyAsync(id, currentUser.Id, dto);
            return Ok(review);
        }
    }
}
