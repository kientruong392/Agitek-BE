using System.Security.Claims;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.DTOs.Carts;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgitekBackend.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add")]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            var cartKey = GetCartKey(dto.GuestSessionId);
            await _cartService.AddToCartAsync(cartKey, dto.ProductId.ToString(), dto.Quantity);
            return Ok(await _cartService.GetCartAsync(cartKey));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CartItemDTO>>> GetCart([FromQuery] string? guestSessionId)
        {
            var cartKey = GetCartKey(guestSessionId);
            return Ok(await _cartService.GetCartAsync(cartKey));
        }

        [HttpDelete("{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveFromCart(int productId, [FromQuery] string? guestSessionId)
        {
            var cartKey = GetCartKey(guestSessionId);
            await _cartService.RemoveFromCartAsync(cartKey, productId.ToString());
            return Ok(await _cartService.GetCartAsync(cartKey));
        }

        [HttpPost("merge")]
        public async Task<IActionResult> MergeCart([FromBody] MergeCartDTO dto)
        {
            var userId = GetAuthenticatedUserId();
            var userCartKey = $"cart:{userId}";
            await _cartService.MergeCartAsync(GetGuestCartKey(dto.GuestSessionId), userCartKey);
            return Ok(await _cartService.GetCartAsync(userCartKey));
        }

        private string GetCartKey(string? guestSessionId)
        {
            if (User.Identity?.IsAuthenticated == true)
                return $"cart:{GetAuthenticatedUserId()}";

            return GetGuestCartKey(guestSessionId);
        }

        private int GetAuthenticatedUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("id");
            if (!int.TryParse(idClaim, out var id) || id <= 0)
                throw new UnauthorizedException("User ID not found in token");

            return id;
        }

        private static string GetGuestCartKey(string? guestSessionId)
        {
            if (!Guid.TryParse(guestSessionId, out var sessionId))
                throw new BadRequestException("A valid guest session ID is required");
            return $"cart:{sessionId:N}";
        }
    }
}
